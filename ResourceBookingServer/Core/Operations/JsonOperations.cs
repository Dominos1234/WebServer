using Core.Diagnostic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Operations
{
	class JsonOperations
	{
		public static string SerializeObjectWithCameCase(object objectToSerialize)
		{
			return JsonConvert.SerializeObject(objectToSerialize, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			});
		}

		public static T DeserializeObjectWithCameCase<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			});
		}

		public static bool TryDeserializeObjectWithCameCase<T>(string json, out T value)
		{
			try
			{
				value = DeserializeObjectWithCameCase<T>(json);
				return value != null;
			}
			catch (JsonException exception)
			{
				Logger.Log("JsonOperations", $"TryDeserializeObjectWithCameCase: Exception {exception}");

				value = default(T);
				return false;
			}
		}
	}
}
