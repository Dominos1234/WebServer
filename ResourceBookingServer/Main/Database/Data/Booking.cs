using MongoDB.Bson;
using System;

namespace ResourceBookingServer.Database.Data
{
	class Booking
	{
		public ObjectId Id { get; set; }
		public ObjectId ItemId { get; set; }
		public ObjectId CategoryId { get; set; }
		public ObjectId UserId { get; set; }

		public string Name { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public DateTime CreationTime { get; set; }

		public int RepeatInterval { get; set; }
		public string RepeatIntervalUnit { get; set; }
		public DateTime? RepeatEndTime { get; set; }

		public bool Confirmed { get; set; }

		public bool IsRepeatIntervalValid()
		{
			if (RepeatInterval == 0 || RepeatEndTime == null)
			{
				return false;
			}

			return RepeatIntervalUnit == "d" || RepeatIntervalUnit == "w" || RepeatIntervalUnit == "m";
		}
	}
}
