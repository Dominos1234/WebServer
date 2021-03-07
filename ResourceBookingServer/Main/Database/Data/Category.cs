using MongoDB.Bson;

namespace ResourceBookingServer.Database.Data
{
	class Category
	{
		public ObjectId Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImageBase64 { get; set; }
		public AttributeName[] Attributes { get; set; }
	}
}
