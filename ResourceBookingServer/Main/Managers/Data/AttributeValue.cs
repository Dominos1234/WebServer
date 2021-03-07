using System;

namespace ResourceBookingServer.Managers.Data
{
	class AttributeValue
	{
		public string Id { get; set; }
		public string Value { get; set; }

		public AttributeValue()
		{

		}
		public AttributeValue(Database.Data.AttributeValue attributeName)
		{
			Id = attributeName.Id.ToString();
			Value = attributeName.Value;
		}

		public Database.Data.AttributeValue ToDatabaseRow()
		{
			return new Database.Data.AttributeValue()
			{
				Id = Guid.Parse(Id),
				Value = Value
			};
		}

		public bool IsValid()
		{
			return Guid.TryParse(Id, out _);
		}
	}
}
