using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers.Data
{
	class Item : IData<Database.Data.Item>
	{
		public string Id { get; set; }
		public string CategoryId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string[] ImagesBase64 { get; set; }
		public IEnumerable<AttributeValue> Attributes { get; set; }

		public bool BookingAuthorizationRequired { get; set; }
		public IEnumerable<string> ResponsibleUsers { get; set; }

		public IEnumerable<Booking> Bookings { get; set; }

		public Item()
		{

		}
		public Item(Database.Data.Item item)
		{
			Id = item.Id.ToString();
			CategoryId = item.CategoryId.ToString();
			Name = item.Name;
			Description = item.Description;
			ImagesBase64 = item.ImagesBase64;
			Attributes = item.Attributes.Select(row => new AttributeValue(row));

			BookingAuthorizationRequired = item.BookingAuthorizationRequired;
			ResponsibleUsers = item.ResponsibleUsers.Select(row => row.ToString());
		}
		public Item(Database.Data.Item item, IEnumerable<Booking> bookings) : this(item)
		{
			Bookings = bookings.SelectMany(booking => booking.GetBookingsWithRepeat(true));
		}

		public FilterDefinition<Database.Data.Item> GetKeyFilter()
		{
			return Builders<Database.Data.Item>.Filter.Eq("Name", Name);
		}

		public Database.Data.Item ToDatabaseRow()
		{
			return new Database.Data.Item()
			{
				Name = Name,
				Description = Description,
				ImagesBase64 = ImagesBase64,
				Attributes = Attributes.Select(row => row.ToDatabaseRow()).ToArray(),

				BookingAuthorizationRequired = BookingAuthorizationRequired,
				ResponsibleUsers = ResponsibleUsers.Select(row => new ObjectId(row)).ToArray()
			};
		}

		public IData<Database.Data.Item> FromDatabaseRow(Database.Data.Item row)
		{
			return new Item(row);
		}

		public UpdateDefinition<Database.Data.Item> GetUpdateDefinition()
		{
			return Builders<Database.Data.Item>.Update
				.Set("Name", Name)
				.Set("Description", Description)
				.Set("ImagesBase64", ImagesBase64)
				.Set("Attributes", Attributes.Select(row => row.ToDatabaseRow()).ToArray())
				.Set("BookingAuthorizationRequired", BookingAuthorizationRequired)
				.Set("ResponsibleUsers", ResponsibleUsers.Select(row => new ObjectId(row)).ToArray());
		}

		public bool IsValid(bool createMode)
		{
			return !string.IsNullOrEmpty(Name) && Attributes != null && ResponsibleUsers != null;
		}
	}
}
