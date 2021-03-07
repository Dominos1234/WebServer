using Core.Network.Http.Native;
using Core.Network.Http.Native.Manager;
using Core.Operations;
using MongoDB.Bson;
using MongoDB.Driver;
using ResourceBookingServer.Container;
using ResourceBookingServer.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers
{
	abstract class Manager : BaseManager
	{
		private readonly SessionService[] sessionServices;

		protected string GetLoggedUser(RequestParameters requestParameters)
		{
			if (!requestParameters.Headers.Data.ContainsKey("session-id"))
			{
				return null;
			}

			var sessionId = requestParameters.Headers.Data["session-id"];

			foreach (var sessionService in sessionServices)
			{
				var loggedUser = sessionService.GetLoggedUser(sessionId);

				if (loggedUser != null)
				{
					return loggedUser;
				}
			}

			return null;
		}

		protected bool IsLogged(RequestParameters requestParameters)
		{
			return GetLoggedUser(requestParameters) != null;
		}

		protected Response HandleGetRequest<TDatabaseType, TMangerType>(RequestParameters requestParameters,
			IMongoCollection<TDatabaseType> collection)
			where TMangerType : IData<TDatabaseType>, new()
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			var data = collection
				.FindSync(Builders<TDatabaseType>.Filter.Empty)
				.ToEnumerable()
				.Select(row => new TMangerType().FromDatabaseRow(row));

			return Response.correct.WithData(data);
		}

		protected Response HandleCreateRequest<TDatabaseType, TMangerType>(RequestParameters requestParameters,
			IMongoCollection<TDatabaseType> collection,
			Func<TDatabaseType, TDatabaseType> dataProvider = null,
			Func<TMangerType, Response> errorProvider = null)
			where TMangerType : IData<TDatabaseType>
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			if (!JsonOperations.TryDeserializeObjectWithCameCase(requestParameters.PostData.Text, out TMangerType managerObject) || !managerObject.IsValid(true))
			{
				return Response.invalidData;
			}

			var keyFilter = managerObject.GetKeyFilter();

			if (keyFilter != null)
			{
				var databaseObject = collection
					.FindSync(keyFilter)
					.FirstOrDefault();

				if (databaseObject != null)
				{
					return Response.entityAlreadyExists;
				}
			}

			if (errorProvider != null)
			{
				var error = errorProvider(managerObject);

				if (error != null)
				{
					return error;
				}
			}

			var databaseRow = managerObject.ToDatabaseRow();

			if (dataProvider != null)
			{
				databaseRow = dataProvider(databaseRow);
			}

			collection.InsertOne(databaseRow);

			return Response.correct;
		}

		protected Response HandleUpdateRequest<TDatabaseType, TMangerType>(RequestParameters requestParameters,
			Dictionary<string, string> pathValues,
			IMongoCollection<TDatabaseType> collection,
			Func<TMangerType, TDatabaseType, TMangerType> dataProvider = null,
			Func<TMangerType, TDatabaseType, Response> errorProvider = null)
			where TMangerType : IData<TDatabaseType>
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			if (!JsonOperations.TryDeserializeObjectWithCameCase(requestParameters.PostData.Text, out TMangerType managerObject) || !managerObject.IsValid(false))
			{
				return Response.invalidData;
			}

			var id = new ObjectId(pathValues["id"]);

			var databaseObjectFilter = Builders<TDatabaseType>.Filter.Eq("Id", id);
			var databaseObject = collection
				.FindSync(databaseObjectFilter)
				.FirstOrDefault();

			if (databaseObject == null)
			{
				return Response.entityNotExists;
			}

			var keyFilter = managerObject.GetKeyFilter();

			if (keyFilter != null)
			{
				var conflictDatabaseObject = collection
					.FindSync(Builders<TDatabaseType>.Filter.Ne("Id", id) & managerObject.GetKeyFilter())
					.FirstOrDefault();

				if (conflictDatabaseObject != null)
				{
					return Response.entityAlreadyExists;
				}
			}

			if (errorProvider != null)
			{
				var error = errorProvider(managerObject, databaseObject);

				if (error != null)
				{
					return error;
				}
			}

			if (dataProvider != null)
			{
				managerObject = dataProvider(managerObject, databaseObject);
			}

			collection.UpdateOne(databaseObjectFilter, managerObject.GetUpdateDefinition());

			return Response.correct;
		}

		protected Response HandleDeleteRequest<TDatabaseType, TMangerType>(RequestParameters requestParameters,
			Dictionary<string, string> pathValues,
			IMongoCollection<TDatabaseType> collection,
			Action<ObjectId> dataRemover = null,
			Func<TDatabaseType, Response> errorProvider = null)
			where TMangerType : IData<TDatabaseType>
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			var id = new ObjectId(pathValues["id"]);
			var filter = Builders<TDatabaseType>.Filter.Eq("Id", id);

			if (errorProvider != null)
			{
				var databaseObject = collection
				.FindSync(filter)
				.FirstOrDefault();

				var error = errorProvider(databaseObject);

				if (error != null)
				{
					return error;
				}
			}

			dataRemover?.Invoke(id);

			collection.DeleteOne(filter);

			return Response.correct;
		}

		public Manager(params SessionService[] sessionServices)
		{
			this.sessionServices = sessionServices;
		}
	}
}
