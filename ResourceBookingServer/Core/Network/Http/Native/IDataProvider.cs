namespace Core.Network.Http.Native
{
	interface IDataProvider
	{
		ResponseParameters IsPathSupported(RequestParameters requestParameters);
	}
}
