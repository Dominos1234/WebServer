using MongoDB.Bson;

namespace ResourceBookingServer.Database.Data
{
	class Item
	{
		public ObjectId Id { get; set; }
		public ObjectId CategoryId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string[] ImagesBase64 { get; set; }
		public AttributeValue[] Attributes { get; set; }

		public bool BookingAuthorizationRequired { get; set; }
		public ObjectId[] ResponsibleUsers { get; set; }
	}
}
