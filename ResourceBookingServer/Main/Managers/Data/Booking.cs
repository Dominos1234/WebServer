using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResourceBookingServer.Managers.Data
{
	class Booking : IData<Database.Data.Booking>
	{
		private int? repeatIntervalValue;

		public string Id { get; set; }
		public string ItemId { get; set; }
		public string CategoryId { get; set; }
		public string UserId { get; set; }

		public string Name { get; set; }
		public string User { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public DateTime CreationTime { get; set; }

		public string RepeatInterval { get; set; }
		public string RepeatIntervalUnit { get; set; }
		public DateTime? RepeatEndTime { get; set; }

		public bool Confirmed { get; set; }

		private int RepeatIntervalValue
		{
			get
			{
				if (repeatIntervalValue != null)
				{
					return repeatIntervalValue.Value;
				}

				if (!int.TryParse(RepeatInterval, out var repeatInterval))
				{
					repeatInterval = 0;
				}

				repeatIntervalValue = repeatInterval;

				return repeatInterval;
			}
		}

		public Booking()
		{

		}
		public Booking(Database.Data.Booking booking)
		{
			Id = booking.Id.ToString();
			ItemId = booking.ItemId.ToString();
			CategoryId = booking.CategoryId.ToString();
			UserId = booking.UserId.ToString();

			Name = booking.Name;

			StartTime = booking.StartTime;
			EndTime = booking.EndTime;
			CreationTime = booking.CreationTime;

			RepeatInterval = booking.RepeatInterval == 0 ? "" : booking.RepeatInterval.ToString();
			RepeatIntervalUnit = booking.RepeatIntervalUnit;
			RepeatEndTime = booking.RepeatEndTime;

			Confirmed = booking.Confirmed;
		}
		public Booking(Database.Data.Booking booking, IEnumerable<Database.Data.User> users) : this(booking)
		{
			var user = users
				.Where(row => row.Id == booking.UserId)
				.FirstOrDefault();

			if (user != null)
			{
				User = $"{user.ExternalId} ({user.Login})";
			}
		}
		public Booking(Booking booking)
		{
			Id = booking.Id;
			ItemId = booking.ItemId;
			CategoryId = booking.CategoryId;
			UserId = booking.UserId;

			Name = booking.Name;
			User = booking.User;

			StartTime = booking.StartTime;
			EndTime = booking.EndTime;
			CreationTime = booking.CreationTime;

			RepeatInterval = booking.RepeatInterval;
			RepeatIntervalUnit = booking.RepeatIntervalUnit;
			RepeatEndTime = booking.RepeatEndTime;

			Confirmed = booking.Confirmed;
		}

		public FilterDefinition<Database.Data.Booking> GetKeyFilter()
		{
			return null;
		}

		public Database.Data.Booking ToDatabaseRow()
		{
			return new Database.Data.Booking()
			{
				StartTime = StartTime,
				EndTime = EndTime,
				CreationTime = DateTime.UtcNow,

				Name = Name,

				RepeatInterval = RepeatIntervalValue,
				RepeatIntervalUnit = RepeatIntervalUnit,
				RepeatEndTime = RepeatEndTime
			};
		}

		public IData<Database.Data.Booking> FromDatabaseRow(Database.Data.Booking row)
		{
			return new Booking(row);
		}

		public UpdateDefinition<Database.Data.Booking> GetUpdateDefinition()
		{
			return Builders<Database.Data.Booking>.Update.Set("Confirmed", Confirmed);
		}

		public bool IsValid(bool createMode)
		{
			return !string.IsNullOrEmpty(Name) && StartTime >= DateTime.UtcNow && EndTime > StartTime;
		}

		public bool IsValidRepeatInterval()
		{
			if (RepeatIntervalValue <= 0 || RepeatEndTime == null || RepeatEndTime < StartTime)
			{
				return false;
			}

			return RepeatIntervalUnit == "d" || RepeatIntervalUnit == "w" || RepeatIntervalUnit == "m";
		}

		public List<Booking> GetBookingsWithRepeat(bool fillRows)
		{
			if (!IsValidRepeatInterval())
			{
				return new List<Booking>() { this };
			}

			var bookings = new List<Booking>();
			var duration = EndTime - StartTime;
			var date = StartTime;

			while (date <= RepeatEndTime)
			{
				var booking = fillRows ? new Booking(this) : new Booking();

				booking.StartTime = date;
				booking.EndTime = date + duration;

				bookings.Add(booking);

				if (RepeatIntervalUnit == "d")
				{
					date = date.AddDays(RepeatIntervalValue);
				}
				else if (RepeatIntervalUnit == "w")
				{
					date = date.AddDays(RepeatIntervalValue * 7);
				}
				else
				{
					date = date.AddMonths(RepeatIntervalValue);
				}
			}

			return bookings;
		}

		public bool ExistsInSameTime(Booking booking)
		{
			return booking.StartTime < EndTime && StartTime < booking.EndTime;
		}
	}
}
