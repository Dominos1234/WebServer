using MongoDB.Driver;
using System;

namespace ResourceBookingServer.Managers.Data
{
	class User : IData<Database.Data.User>
	{
		public string Id { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string ExternalId { get; set; }
		public bool AdminUser { get; set; }

		public User()
		{

		}
		public User(Database.Data.User user)
		{
			Id = user.Id.ToString();
			Login = user.Login;
			ExternalId = user.ExternalId;
			AdminUser = user.AdminUser;
		}

		public FilterDefinition<Database.Data.User> GetKeyFilter()
		{
			return Builders<Database.Data.User>.Filter.Eq("Login", Login);
		}

		public Database.Data.User ToDatabaseRow()
		{
			return new Database.Data.User()
			{
				Login = Login,
				Password = Password,
				ExternalId = ExternalId,
				AdminUser = AdminUser
			};
		}

		public IData<Database.Data.User> FromDatabaseRow(Database.Data.User row)
		{
			return new User(row);
		}

		public UpdateDefinition<Database.Data.User> GetUpdateDefinition()
		{
			var fields = new Tuple<string, string>[]
			{
				new Tuple<string, string>("Login", Login),
				new Tuple<string, string>("ExternalId", ExternalId),
				new Tuple<string, string>("Password", Password)
			};

			var definition = Builders<Database.Data.User>.Update
				.Set("AdminUser", AdminUser);

			foreach (var field in fields)
			{
				if (!string.IsNullOrEmpty(field.Item2))
				{
					definition = definition.Set(field.Item1, field.Item2);
				}
			}

			return definition;
		}

		public bool IsValid(bool createMode)
		{
			return !string.IsNullOrEmpty(Login) &&
				!string.IsNullOrEmpty(ExternalId) &&
				(!createMode || !string.IsNullOrEmpty(Password));
		}
	}
}
