using Core.Network.Http.Native;
using Core.Network.Http.Native.Manager;
using Core.Operations;
using MongoDB.Bson;
using MongoDB.Driver;
using ResourceBookingServer.Container;
using ResourceBookingServer.Database;
using ResourceBookingServer.Managers.Data;
using ResourceBookingServer.Service;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers
{
	class UserManager : Manager
	{
		private readonly Db db;
		private readonly SessionService sessionService;
		private readonly PasswordService passwordService;

		[RequestHandler("/login", RequestHeaders.MethodType.Post)]
		private object OnLoginRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			var postData = requestParameters.PostData.Dictionary;

			if (!StandardDataOperations.AreKeysInDictionary(postData, new string[] { "login", "password" }))
			{
				return Response.missingFields;
			}

			var login = postData["login"];
			var password = passwordService.GetHash(postData["password"]);

			var filter = Builders<Database.Data.User>.Filter.Eq("Login", login) &
				Builders<Database.Data.User>.Filter.Eq("Password", password);

			db.UsersLock.EnterReadLock();

			try
			{
				var user = db.Users
					.FindSync(filter)
					.FirstOrDefault();

				if (user != null)
				{
					var sessionId = sessionService.CreateUserSession(login);

					return Response.correct.WithData(new User(user)
					{
						Password = sessionId
					});
				}

				return Response.incorrectLogin;
			}
			finally
			{
				db.UsersLock.ExitReadLock();
			}
		}

		[RequestHandler("/category", RequestHeaders.MethodType.Get)]
		private object OnGetCategoriesRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			return HandleGetRequest<Database.Data.Category, Category>(requestParameters, db.Categories);
		}

		[RequestHandler("/category/{id}", RequestHeaders.MethodType.Get)]
		private object OnGetCategoryRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			var id = new ObjectId(pathValues["id"]);

			var category = db.Categories.FindSync(Builders<Database.Data.Category>.Filter.Eq("Id", id)).FirstOrDefault();

			if (category == null)
			{
				return Response.entityNotExists;
			}

			var users = db.Users
				.FindSync(Builders<Database.Data.User>.Filter.Empty)
				.ToList();

			var bookings = db.Bookings
				.FindSync(Builders<Database.Data.Booking>.Filter.Eq("CategoryId", id))
				.ToEnumerable()
				.Select(row => new Booking(row, users))
				.ToList();

			var items = db.Items
				.FindSync(Builders<Database.Data.Item>.Filter.Eq("CategoryId", id))
				.ToEnumerable()
				.Select(row =>
				{
					var itemId = row.Id.ToString();

					return new Item(row, bookings.FindAll(booking => booking.ItemId == itemId));
				});

			return Response.correct.WithData(new Category(category, items));
		}

		[RequestHandler("/category/{categoryId}/item/{itemId}/booking", RequestHeaders.MethodType.Post)]
		private object OnCreateBookingRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.ItemsLock.EnterReadLock();
			db.UsersLock.EnterReadLock();
			db.BookingsLock.EnterWriteLock();

			try
			{
				Database.Data.User user = null;

				var loggedUser = GetLoggedUser(requestParameters);

				if (!string.IsNullOrEmpty(loggedUser))
				{
					user = db.Users
						.FindSync(Builders<Database.Data.User>.Filter.Eq("Login", loggedUser))
						.FirstOrDefault();
				}

				var itemId = new ObjectId(pathValues["itemId"]);

				var item = db.Items
					.FindSync(Builders<Database.Data.Item>.Filter.Eq("Id", itemId))
					.FirstOrDefault();

				return HandleCreateRequest<Database.Data.Booking, Booking>(requestParameters, db.Bookings, (booking) =>
				{
					booking.ItemId = itemId;
					booking.CategoryId = new ObjectId(pathValues["categoryId"]);
					booking.UserId = user.Id;
					booking.Confirmed = !item.BookingAuthorizationRequired;

					if (!booking.IsRepeatIntervalValid())
					{
						booking.RepeatInterval = 0;
						booking.RepeatIntervalUnit = null;
						booking.RepeatEndTime = null;
					}

					return booking;
				}, (booking) =>
				{
					if (string.IsNullOrEmpty(loggedUser))
					{
						return Response.masterAdminNotAllowed;
					}

					if (user == null)
					{
						return Response.notLogged;
					}

					if (item == null)
					{
						return Response.entityNotExists;
					}

					var existingBookings = db.Bookings
						.FindSync(Builders<Database.Data.Booking>.Filter.Eq("ItemId", itemId))
						.ToEnumerable()
						.Select(row => new Booking(row))
						.ToList();

					var isValid = booking
						.GetBookingsWithRepeat(false)
						.All(row => existingBookings.All(existingRow => !existingRow.ExistsInSameTime(row)));

					if (!isValid)
					{
						return Response.bookingExistingInSameInterval;
					}

					return null;
				});
			}
			finally
			{
				db.ItemsLock.ExitReadLock();
				db.UsersLock.ExitReadLock();
				db.BookingsLock.ExitWriteLock();
			}
		}

		[RequestHandler("/category/{categoryId}/item/{itemId}/booking/{id}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteBookingRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.BookingsLock.EnterWriteLock();

			try
			{
				return HandleDeleteRequest<Database.Data.Booking, Booking>(requestParameters, pathValues, db.Bookings, errorProvider: (booking) =>
				{
					Database.Data.User user = null;

					var loggedUser = GetLoggedUser(requestParameters);

					if (!string.IsNullOrEmpty(loggedUser))
					{
						user = db.Users
							.FindSync(Builders<Database.Data.User>.Filter.Eq("Login", loggedUser))
							.FirstOrDefault();
					}

					if (user == null)
					{
						return Response.notLogged;
					}

					if (booking.UserId != user.Id)
					{
						return Response.missingPermissions;
					}

					return null;
				});
			}
			finally
			{
				db.BookingsLock.ExitWriteLock();
			}
		}

		protected override string GetSupportedPathPrefix()
		{
			return "/api/user";
		}

		public UserManager(Db db, SessionService sessionService, PasswordService passwordService) : base(sessionService)
		{
			this.db = db;
			this.sessionService = sessionService;
			this.passwordService = passwordService;
		}
	}
}
