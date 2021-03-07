namespace Core.Network.Http.Native.Authentication
{
	interface IChecker
	{
		bool IsAuthenticated(RequestParameters requestParameters);

		ResponseParameters GetErrorResponse();
	}
}
