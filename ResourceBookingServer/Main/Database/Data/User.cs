using MongoDB.Bson;

namespace ResourceBookingServer.Database.Data
{
	class User
	{
		public ObjectId Id { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string ExternalId { get; set; }
		public bool AdminUser { get; set; }
	}
}
