using Core.Network.Http.Native;
using Core.Network.Http.Native.Manager;
using MongoDB.Driver;
using ResourceBookingServer.Container;
using ResourceBookingServer.Database;
using ResourceBookingServer.Managers.Data;
using ResourceBookingServer.Service;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers
{
	class MasterAdminManager : Manager
	{
		private readonly Db db;
		private readonly SessionService sessionService;
		private readonly PasswordService passwordService;
		private readonly string masterKey;

		[RequestHandler("/login", RequestHeaders.MethodType.Post)]
		private object OnLoginRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			var postData = requestParameters.PostData.Dictionary;

			if (!postData.ContainsKey("key"))
			{
				return Response.missingFields;
			}

			var key = postData["key"];

			if (key == masterKey)
			{
				var sessionId = sessionService.CreateUserSession("");

				return Response.correct.WithData(new User()
				{
					Password = sessionId
				});
			}

			return Response.incorrectLogin;
		}

		[RequestHandler("/user", RequestHeaders.MethodType.Get)]
		private object OnGetUsersRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			return HandleGetRequest<Database.Data.User, User>(requestParameters, db.Users);
		}

		[RequestHandler("/user", RequestHeaders.MethodType.Post)]
		private object OnCreateUserRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.UsersLock.EnterWriteLock();

			try
			{
				return HandleCreateRequest<Database.Data.User, User>(requestParameters, db.Users, (row) =>
				{
					row.Password = passwordService.GetHash(row.Password);

					return row;
				});
			}
			finally
			{
				db.UsersLock.ExitWriteLock();
			}
		}

		[RequestHandler("/user/{id}", RequestHeaders.MethodType.Post)]
		private object OnUpdateUserRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.UsersLock.EnterWriteLock();

			try
			{
				return HandleUpdateRequest<Database.Data.User, User>(requestParameters, pathValues, db.Users, (managerObject, databaseObject) =>
				{
					if (!string.IsNullOrEmpty(managerObject.Password))
					{
						managerObject.Password = passwordService.GetHash(managerObject.Password);
					}

					return managerObject;
				});
			}
			finally
			{
				db.UsersLock.ExitWriteLock();
			}
		}

		[RequestHandler("/user/{id}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteUserRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.ItemsLock.EnterWriteLock();
			db.UsersLock.EnterWriteLock();
			db.BookingsLock.EnterWriteLock();

			try
			{
				return HandleDeleteRequest<Database.Data.User, User>(requestParameters, pathValues, db.Users, (id) =>
				{
					var items = db.Items
						.FindSync(Builders<Database.Data.Item>.Filter.Where(item => item.ResponsibleUsers.Contains(id)))
						.ToList();

					foreach (var item in items)
					{
						var responsibleUsers = item.ResponsibleUsers
							.Where(user => user != id)
							.ToArray();

						var updateDefinition = Builders<Database.Data.Item>.Update.Set("ResponsibleUsers", responsibleUsers);

						db.Items.UpdateOne(Builders<Database.Data.Item>.Filter.Eq("Id", item.Id), updateDefinition);
					}

					db.Bookings.DeleteMany(Builders<Database.Data.Booking>.Filter.Eq("UserId", id));
				});
			}
			finally
			{
				db.ItemsLock.ExitWriteLock();
				db.UsersLock.ExitWriteLock();
				db.BookingsLock.ExitWriteLock();
			}
		}

		protected override string GetSupportedPathPrefix()
		{
			return "/api/master-admin";
		}

		public MasterAdminManager(Db db, SessionService sessionService, PasswordService passwordService, string masterKey) : base(sessionService)
		{
			this.db = db;
			this.sessionService = sessionService;
			this.passwordService = passwordService;
			this.masterKey = masterKey;
		}
	}
}
