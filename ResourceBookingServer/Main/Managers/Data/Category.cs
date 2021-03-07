using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers.Data
{
	class Category : IData<Database.Data.Category>
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImageBase64 { get; set; }
		public IEnumerable<AttributeName> Attributes { get; set; }
		public IEnumerable<Item> Items { get; set; }

		public Category()
		{

		}
		public Category(Database.Data.Category category)
		{
			Id = category.Id.ToString();
			Name = category.Name;
			Description = category.Description;
			ImageBase64 = category.ImageBase64;
			Attributes = category.Attributes.Select(row => new AttributeName(row));
		}
		public Category(Database.Data.Category category, IEnumerable<Item> items) : this(category)
		{
			Items = items;
		}

		public FilterDefinition<Database.Data.Category> GetKeyFilter()
		{
			return Builders<Database.Data.Category>.Filter.Eq("Name", Name);
		}

		public Database.Data.Category ToDatabaseRow()
		{
			return new Database.Data.Category()
			{
				Name = Name,
				Description = Description,
				ImageBase64 = ImageBase64,
				Attributes = Attributes.Select(row => row.ToDatabaseRow()).ToArray()
			};
		}

		public IData<Database.Data.Category> FromDatabaseRow(Database.Data.Category row)
		{
			return new Category(row);
		}

		public UpdateDefinition<Database.Data.Category> GetUpdateDefinition()
		{
			return Builders<Database.Data.Category>.Update
				.Set("Name", Name)
				.Set("Description", Description)
				.Set("ImageBase64", ImageBase64);
		}

		public bool IsValid(bool createMode)
		{
			return !string.IsNullOrEmpty(Name) && Attributes != null;
		}
	}
}
