using Core.Network.Http.Native.Authentication;
using Core.Network.SocketServer;
using System;

namespace Core.Network.Http.Native
{
	class HttpServer
	{
		public HttpServer(string resourcesPath, int port = 80, bool startAsynchronous = false, bool enableMaxTimeWithoutDataTillTimeout = true, Func<IDataProvider>[] httpServerDataProviders = null, IChecker authenticationChecker = null)
		{
			new TcpSocketServer(() =>
			{
				return new DataProcessor(resourcesPath, enableMaxTimeWithoutDataTillTimeout, httpServerDataProviders, authenticationChecker);
			}, port, startAsynchronous);
		}
	}
}
