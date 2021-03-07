using Core.Network.Http.Native;
using Core.Network.Http.Native.Manager;
using Core.Operations;
using MongoDB.Bson;
using MongoDB.Driver;
using ResourceBookingServer.Container;
using ResourceBookingServer.Database;
using ResourceBookingServer.Managers.Data;
using ResourceBookingServer.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers
{
	class AdminManager : Manager
	{
		private readonly Db db;
		private readonly SessionService sessionService;
		private readonly PasswordService passwordService;
		private readonly QRCodeService qrCodeService;
		private readonly string qrCodeUrl;

		private object UpdateCategoryAttributes(RequestParameters requestParameters, Dictionary<string, string> pathValues, Func<Database.Data.AttributeName[], Database.Data.AttributeName[]> dataProvider)
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			db.CategoriesLock.EnterWriteLock();

			try
			{
				var id = new ObjectId(pathValues["categoryId"]);

				var databaseObjectFilter = Builders<Database.Data.Category>.Filter.Eq("Id", id);
				var databaseObject = db.Categories
					.FindSync(databaseObjectFilter)
					.FirstOrDefault();

				if (databaseObject == null)
				{
					return Response.entityNotExists;
				}

				var attributes = dataProvider(databaseObject.Attributes);
				var updateDefinition = Builders<Database.Data.Category>.Update.Set("Attributes", attributes);

				db.Categories.UpdateOne(databaseObjectFilter, updateDefinition);

				return Response.correct;
			}
			finally
			{
				db.CategoriesLock.ExitWriteLock();
			}
		}
		private object UpdateItemImages(RequestParameters requestParameters, Dictionary<string, string> pathValues, Func<List<string>, List<string>> dataProvider)
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			db.ItemsLock.EnterWriteLock();

			try
			{
				var id = new ObjectId(pathValues["itemId"]);

				var databaseObjectFilter = Builders<Database.Data.Item>.Filter.Eq("Id", id);
				var databaseObject = db.Items
					.FindSync(databaseObjectFilter)
					.FirstOrDefault();

				if (databaseObject == null)
				{
					return Response.entityNotExists;
				}

				var previousImagesBase64 = new List<string>(databaseObject.ImagesBase64);
				var imagesBase64 = dataProvider(previousImagesBase64).ToArray();
				var updateDefinition = Builders<Database.Data.Item>.Update.Set("ImagesBase64", imagesBase64);

				db.Items.UpdateOne(databaseObjectFilter, updateDefinition);

				return Response.correct;
			}
			finally
			{
				db.ItemsLock.ExitWriteLock();
			}
		}

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
				Builders<Database.Data.User>.Filter.Eq("Password", password) &
				Builders<Database.Data.User>.Filter.Eq("AdminUser", true);

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
					row.AdminUser = false;

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

					managerObject.AdminUser = databaseObject.AdminUser;

					return managerObject;
				}, (managerObject, databaseObject) =>
				{
					var loggedUser = GetLoggedUser(requestParameters);

					if (!databaseObject.AdminUser)
					{
						return null;
					}

					if (loggedUser != databaseObject.Login)
					{
						return Response.missingPermissions;
					}

					return null;
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
			db.UsersLock.EnterWriteLock();

			try
			{
				return HandleDeleteRequest<Database.Data.User, User>(requestParameters, pathValues, db.Users, errorProvider: (databaseObject) =>
				{
					if (databaseObject == null || !databaseObject.AdminUser)
					{
						return null;
					}

					return Response.missingPermissions;
				});
			}
			finally
			{
				db.UsersLock.ExitWriteLock();
			}
		}

		[RequestHandler("/category", RequestHeaders.MethodType.Get)]
		private object OnGetCategoriesRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			return HandleGetRequest<Database.Data.Category, Category>(requestParameters, db.Categories);
		}

		[RequestHandler("/category", RequestHeaders.MethodType.Post)]
		private object OnCreateCategoryRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.CategoriesLock.EnterWriteLock();

			try
			{
				return HandleCreateRequest<Database.Data.Category, Category>(requestParameters, db.Categories);
			}
			finally
			{
				db.CategoriesLock.ExitWriteLock();
			}
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

		[RequestHandler("/category/{id}", RequestHeaders.MethodType.Post)]
		private object OnUpdateCategoryRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.CategoriesLock.EnterWriteLock();

			try
			{
				return HandleUpdateRequest<Database.Data.Category, Category>(requestParameters, pathValues, db.Categories);
			}
			finally
			{
				db.CategoriesLock.ExitWriteLock();
			}
		}

		[RequestHandler("/category/{id}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteCategoryRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.CategoriesLock.EnterWriteLock();

			try
			{
				return HandleDeleteRequest<Database.Data.Category, Category>(requestParameters, pathValues, db.Categories);
			}
			finally
			{
				db.CategoriesLock.ExitWriteLock();
			}
		}

		[RequestHandler("/category/{categoryId}/attribute", RequestHeaders.MethodType.Post)]
		private object OnCreateCategoryAttributeRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!JsonOperations.TryDeserializeObjectWithCameCase<AttributeName>(requestParameters.PostData.Text, out var value) ||
				!value.IsValid())
			{
				return Response.invalidData;
			}

			return UpdateCategoryAttributes(requestParameters, pathValues, (attributes) =>
			{
				var data = attributes.ToList();

				data.Add(new Database.Data.AttributeName()
				{
					Id = Guid.NewGuid(),
					Name = value.Name
				});

				return data.ToArray();
			});
		}

		[RequestHandler("/category/{categoryId}/attribute/{id}", RequestHeaders.MethodType.Post)]
		private object OnUpdateCategoryAttributeRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!JsonOperations.TryDeserializeObjectWithCameCase<AttributeName>(requestParameters.PostData.Text, out var value) ||
				!value.IsValid() ||
				!Guid.TryParse(pathValues["id"], out var id))
			{
				return Response.invalidData;
			}

			return UpdateCategoryAttributes(requestParameters, pathValues, (attributes) =>
			{
				foreach (var row in attributes)
				{
					if (row.Id == id)
					{
						row.Name = value.Name;
						break;
					}
				}

				return attributes;
			});
		}

		[RequestHandler("/category/{categoryId}/attribute/{id}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteCategoryAttributeRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!Guid.TryParse(pathValues["id"], out var id))
			{
				return Response.invalidData;
			}

			return UpdateCategoryAttributes(requestParameters, pathValues, (attributes) =>
			{
				return attributes
					.Where(row => row.Id != id)
					.ToArray();
			});
		}

		[RequestHandler("/category/{categoryId}/item", RequestHeaders.MethodType.Post)]
		private object OnCreateItemRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.ItemsLock.EnterWriteLock();
			db.UsersLock.EnterReadLock();

			try
			{
				return HandleCreateRequest<Database.Data.Item, Item>(requestParameters, db.Items, (item) =>
				{
					var users = db.Users
						.FindSync(Builders<Database.Data.User>.Filter.Empty)
						.ToEnumerable()
						.Select(row => row.Id);

					var usersSet = new HashSet<ObjectId>(users);

					item.CategoryId = new ObjectId(pathValues["categoryId"]);
					item.ResponsibleUsers = item.ResponsibleUsers
						.Distinct()
						.Where(row => usersSet.Contains(row))
						.ToArray();

					return item;
				});
			}
			finally
			{
				db.ItemsLock.ExitWriteLock();
				db.UsersLock.ExitReadLock();
			}
		}

		[RequestHandler("/category/{categoryId}/item/{id}", RequestHeaders.MethodType.Get)]
		private object OnGetItemRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			var id = new ObjectId(pathValues["id"]);

			var item = db.Items.FindSync(Builders<Database.Data.Item>.Filter.Eq("Id", id)).FirstOrDefault();

			if (item == null)
			{
				return Response.entityNotExists;
			}

			return Response.correct.WithData(new Item(item));
		}

		[RequestHandler("/category/{categoryId}/item/{id}", RequestHeaders.MethodType.Post)]
		private object OnUpdateItemRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.ItemsLock.EnterWriteLock();
			db.UsersLock.EnterReadLock();

			try
			{
				return HandleUpdateRequest<Database.Data.Item, Item>(requestParameters, pathValues, db.Items, (managerObject, databaseObject) =>
				{
					var users = db.Users
						.FindSync(Builders<Database.Data.User>.Filter.Empty)
						.ToEnumerable()
						.Select(row => row.Id);

					var usersSet = new HashSet<ObjectId>(users);

					managerObject.ResponsibleUsers = managerObject.ResponsibleUsers
						.Distinct()
						.Where(row => usersSet.Contains(new ObjectId(row)))
						.ToArray();

					return managerObject;
				});
			}
			finally
			{
				db.ItemsLock.ExitWriteLock();
				db.UsersLock.ExitReadLock();
			}
		}

		[RequestHandler("/category/{categoryId}/item/{id}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteItemRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.ItemsLock.EnterWriteLock();

			try
			{
				return HandleDeleteRequest<Database.Data.Item, Item>(requestParameters, pathValues, db.Items);
			}
			finally
			{
				db.ItemsLock.ExitWriteLock();
			}
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

				return HandleCreateRequest<Database.Data.Booking, Booking>(requestParameters, db.Bookings, (booking) =>
				{
					booking.ItemId = itemId;
					booking.CategoryId = new ObjectId(pathValues["categoryId"]);
					booking.UserId = user.Id;
					booking.Confirmed = true;

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

					var item = db.Items
						.FindSync(Builders<Database.Data.Item>.Filter.Eq("Id", itemId))
						.FirstOrDefault();

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

		[RequestHandler("/category/{categoryId}/item/{itemId}/booking/{id}", RequestHeaders.MethodType.Post)]
		private object OnUpdateBookingRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.BookingsLock.EnterWriteLock();

			try
			{
				return HandleUpdateRequest<Database.Data.Booking, Booking>(requestParameters, pathValues, db.Bookings);
			}
			finally
			{
				db.BookingsLock.ExitWriteLock();
			}
		}

		[RequestHandler("/category/{categoryId}/item/{itemId}/booking/{id}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteBookingRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			db.BookingsLock.EnterWriteLock();

			try
			{
				return HandleDeleteRequest<Database.Data.Booking, Booking>(requestParameters, pathValues, db.Bookings);
			}
			finally
			{
				db.BookingsLock.ExitWriteLock();
			}
		}

		[RequestHandler("/category/{categoryId}/item/{itemId}/image", RequestHeaders.MethodType.Post)]
		private object OnCreateItemImageRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			var image = requestParameters.PostData.Text;

			if (string.IsNullOrEmpty(image))
			{
				return Response.invalidData;
			}

			return UpdateItemImages(requestParameters, pathValues, (images) =>
			{
				images.Add(image);

				return images;
			});
		}

		[RequestHandler("/category/{categoryId}/item/{itemId}/image/{index}", RequestHeaders.MethodType.Delete)]
		private object OnDeleteItemImageRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!int.TryParse(pathValues["index"], out var index))
			{
				return Response.invalidData;
			}

			return UpdateItemImages(requestParameters, pathValues, (images) =>
			{
				images.RemoveAt(index);

				return images;
			});
		}

		[RequestHandler("/category/{categoryId}/item/{itemId}/image/{index}", RequestHeaders.MethodType.Post)]
		private object OnMoveItemImageRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!int.TryParse(pathValues["index"], out var index) ||
				!int.TryParse(requestParameters.PostData.Text, out var offset))
			{
				return Response.invalidData;
			}

			return UpdateItemImages(requestParameters, pathValues, (images) =>
			{
				var position = MathOperations.Clamp(index + offset, 0, images.Count - 1);
				var image = images[position];

				images[position] = images[index];
				images[index] = image;

				return images;
			});
		}

		[RequestHandler("/qr-code/{id}", RequestHeaders.MethodType.Get)]
		private object OnGetQRCodeRequest(RequestParameters requestParameters, Dictionary<string, string> pathValues)
		{
			if (!IsLogged(requestParameters))
			{
				return Response.notLogged;
			}

			var id = new ObjectId(pathValues["id"]);
			var url = string.Format(qrCodeUrl, id);
			var qrCode = qrCodeService.GetBase64UrlQRCode(url);

			return Response.correct.WithData(qrCode);
		}

		protected override string GetSupportedPathPrefix()
		{
			return "/api/admin";
		}

		public AdminManager(Db db, SessionService sessionService, PasswordService passwordService, QRCodeService qrCodeService, string qrCodeUrl, SessionService masterAdminSessionService) : base(masterAdminSessionService, sessionService)
		{
			this.db = db;
			this.sessionService = sessionService;
			this.passwordService = passwordService;
			this.qrCodeService = qrCodeService;
			this.qrCodeUrl = qrCodeUrl;
		}
	}
}
