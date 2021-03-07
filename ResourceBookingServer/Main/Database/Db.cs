using MongoDB.Driver;
using ResourceBookingServer.Database.Data;
using System.Threading;

namespace ResourceBookingServer.Database
{
	class Db
	{
		public IMongoCollection<Category> Categories { get; }
		public IMongoCollection<Item> Items { get; }
		public IMongoCollection<User> Users { get; }
		public IMongoCollection<Booking> Bookings { get; }

		public ReaderWriterLockSlim CategoriesLock { get; }
		public ReaderWriterLockSlim ItemsLock { get; }
		public ReaderWriterLockSlim UsersLock { get; }
		public ReaderWriterLockSlim BookingsLock { get; }

		public Db(string connectionString, string databaseName)
		{
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseName);

			Categories = database.GetCollection<Category>("Categories");
			Items = database.GetCollection<Item>("Items");
			Users = database.GetCollection<User>("Users");
			Bookings = database.GetCollection<Booking>("Bookings");

			CategoriesLock = new ReaderWriterLockSlim();
			ItemsLock = new ReaderWriterLockSlim();
			UsersLock = new ReaderWriterLockSlim();
			BookingsLock = new ReaderWriterLockSlim();
		}
	}
}
