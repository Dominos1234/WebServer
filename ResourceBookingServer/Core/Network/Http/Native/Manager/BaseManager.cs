using Core.Diagnostic;
using Core.Operations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Core.Network.Http.Native.Manager
{
	abstract class BaseManager : IDataProvider
	{
		private class RequestHandlerContainer
		{
			public string Path { get; }
			public RequestHeaders.MethodType Method { get; }
			public RequestHandler RequestHandler { get; }

			public RequestHandlerContainer(string path, RequestHeaders.MethodType method, RequestHandler requestHandler)
			{
				Path = path;
				Method = method;
				RequestHandler = requestHandler;
			}
		}

		private delegate object RequestHandler(RequestParameters requestParameters, Dictionary<string, string> pathValues);

		private readonly List<RequestHandlerContainer> requestHandlers;

		protected abstract string GetSupportedPathPrefix();

		private bool IsRequestHandlerPathMatch(string[] pathParts, string patternPath, out Dictionary<string, string> pathValues)
		{
			var patternParts = patternPath.Split('/');

			pathValues = null;

			if (pathParts.Length == patternParts.Length)
			{
				pathValues = new Dictionary<string, string>();

				for (var index = 0; index < pathParts.Length; ++index)
				{
					var path = pathParts[index];
					var pattern = patternParts[index];

					if (pattern.StartsWith("{") && pattern.EndsWith("}"))
					{
						if (path.Length > 0)
						{
							var name = pattern.Substring(1, pattern.Length - 2);

							pathValues[name] = path;
						}
						else
						{
							pathValues = null;
							break;
						}
					}
					else if (path != pattern)
					{
						pathValues = null;
						break;
					}
				}
			}

			return pathValues != null;
		}

		public BaseManager()
		{
			requestHandlers = new List<RequestHandlerContainer>();

			var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (var method in methods)
			{
				var attributes = method.GetCustomAttributes(false);

				foreach (var attribute in attributes)
				{
					if (attribute is RequestHandlerAttribute requestHandlerAttribute)
					{
						var methodDelegate = (RequestHandler)Delegate.CreateDelegate(typeof(RequestHandler), this, method.Name);
						var requestHandler = new RequestHandlerContainer(requestHandlerAttribute.Path, requestHandlerAttribute.Method, methodDelegate);

						requestHandlers.Add(requestHandler);
						break;
					}
				}
			}
		}

		public ResponseParameters IsPathSupported(RequestParameters requestParameters)
		{
			object result = null;

			var supportedPathPrefix = GetSupportedPathPrefix();

			if (requestParameters.RequestedPath.StartsWith(supportedPathPrefix))
			{
				var pathParts = requestParameters.RequestedPath.Substring(supportedPathPrefix.Length).Split('/');

				foreach (var requestHandler in requestHandlers)
				{
					if (requestParameters.Headers.Method == requestHandler.Method && IsRequestHandlerPathMatch(pathParts, requestHandler.Path, out var pathValues))
					{
						try
						{
							result = requestHandler.RequestHandler(requestParameters, pathValues);
							break;
						}
						catch (Exception exception)
						{
							Logger.Log("BaseManager", $"Exception: {exception}");
						}
					}
				}
			}

			if (result == null)
			{
				return null;
			}
			else
			{
				string textResponse;

				if (result is string textResult)
				{
					textResponse = textResult;
				}
				else
				{
					textResponse = JsonOperations.SerializeObjectWithCameCase(result);
				}

				return new ResponseParameters(Encoding.UTF8.GetBytes(textResponse), 200, HttpOperations.ContentType.Json);
			}
		}
	}
}
