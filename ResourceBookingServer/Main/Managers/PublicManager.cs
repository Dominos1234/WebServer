using Core.Network.Http.Native;
using Core.Network.Http.Native.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using ResourceBookingServer.Container;
using ResourceBookingServer.Database;
using ResourceBookingServer.Managers.Data;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers
{
	class PublicManager : Manager
	{
		private readonly Db db;

		[RequestHandler("/item/{id}", RequestHeaders.MethodType.Get)]
		private object OnGetItemRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			var id = new ObjectId(pathValues["id"]);

			var item = db.Items.FindSync(Builders<Database.Data.Item>.Filter.Eq("Id", id)).FirstOrDefault();

			if (item == null)
			{
				return Response.entityNotExists;
			}

			var users = db.Users
				.FindSync(Builders<Database.Data.User>.Filter.Empty)
				.ToList();

			var bookings = db.Bookings
				.FindSync(Builders<Database.Data.Booking>.Filter.Eq("ItemId", id))
				.ToEnumerable()
				.Select(row => new Booking(row, users))
				.ToList();

			return Response.correct.WithData(new Item(item, bookings));
		}

		protected override string GetSupportedPathPrefix()
		{
			return "/api/public";
		}

		public PublicManager(Db db)
		{
			this.db = db;
		}
	}
}
