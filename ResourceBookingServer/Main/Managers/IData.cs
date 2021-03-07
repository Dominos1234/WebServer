using MongoDB.Driver;

namespace ResourceBookingServer.Managers
{
	interface IData<T>
	{
		FilterDefinition<T> GetKeyFilter();

		T ToDatabaseRow();

		IData<T> FromDatabaseRow(T row);

		UpdateDefinition<T> GetUpdateDefinition();

		bool IsValid(bool createMode);
	}
}
