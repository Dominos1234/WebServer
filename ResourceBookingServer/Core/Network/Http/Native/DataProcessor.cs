using Core.Network.Http.Native.Authentication;
using Core.Network.SocketDataProcessor;
using System;
using System.Collections.Generic;

namespace Core.Network.Http.Native
{
	class DataProcessor : SocketBaseDataProcessor
	{
		private static readonly string serverName = "NativeHttpServer";

		private readonly string resourcesPath;
		private readonly bool enableMaxTimeWithoutDataTillTimeout;
		private readonly Func<IDataProvider>[] httpServerDataProviders;
		private readonly IChecker authenticationChecker;

		private readonly List<byte> incomingMessage;
		private readonly RequestParameters requestParameters;

		private List<byte> GetResponse(RequestParameters requestParameters)
		{
			if (httpServerDataProviders != null)
			{
				foreach (var httpServerDataProvider in httpServerDataProviders)
				{
					var result = httpServerDataProvider().IsPathSupported(requestParameters);

					if (result != null)
					{
						return result.ToByteList(serverName);
					}
				}
			}

			if (requestParameters.RequestedPath == "/")
			{
				requestParameters.RequestedPath = "/index.html";
			}

			var responseContent = HttpOperations.GetFileContent(resourcesPath, requestParameters.RequestedPath, false, out var statusCode, out var contentType);

			return new ResponseParameters(responseContent, statusCode, contentType).ToByteList(serverName);
		}

		protected override int MaxTimeWithoutDataTillTimeoutInMilliseconds
		{
			get
			{
				if (!enableMaxTimeWithoutDataTillTimeout)
				{
					return 0;
				}

				return 60 * 1000;
			}
		}

		protected override bool OnDataReadCallback(List<byte> buffer)
		{
			incomingMessage.AddRange(buffer);

			if (requestParameters.Load(incomingMessage))
			{
				if (authenticationChecker == null || authenticationChecker.IsAuthenticated(requestParameters))
				{
					var response = GetResponse(requestParameters);

					if (response != null)
					{
						SendData(response);

						return false;
					}
				}
				else
				{
					SendData(authenticationChecker.GetErrorResponse().ToByteList(serverName));
				}

				requestParameters.Clear();
			}

			return true;
		}

		public DataProcessor(string resourcesPath, bool enableMaxTimeWithoutDataTillTimeout, Func<IDataProvider>[] httpServerDataProviders, IChecker authenticationChecker)
		{
			this.resourcesPath = resourcesPath;
			this.enableMaxTimeWithoutDataTillTimeout = enableMaxTimeWithoutDataTillTimeout;
			this.httpServerDataProviders = httpServerDataProviders;
			this.authenticationChecker = authenticationChecker;

			incomingMessage = new List<byte>();
			requestParameters = new RequestParameters((byte[] dataToSend) =>
			{
				if (IsConnectionOpened)
				{
					if (dataToSend != null)
					{
						SendData(dataToSend);
					}
					else
					{
						CloseSocket();
					}

					return true;
				}
				else
				{
					return false;
				}
			});
		}
	}
}
