using System;

namespace ResourceBookingServer.Managers.Data
{
	class AttributeName
	{
		public string Id { get; set; }
		public string Name { get; set; }

		public AttributeName()
		{

		}
		public AttributeName(Database.Data.AttributeName attributeName)
		{
			Id = attributeName.Id.ToString();
			Name = attributeName.Name;
		}

		public Database.Data.AttributeName ToDatabaseRow()
		{
			return new Database.Data.AttributeName()
			{
				Id = Guid.NewGuid(),
				Name = Name
			};
		}

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Name);
		}
	}
}
