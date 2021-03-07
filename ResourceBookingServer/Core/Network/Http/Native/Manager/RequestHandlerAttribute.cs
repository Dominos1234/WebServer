using Core.Network.Http.Native;
using System;

namespace Core.Network.Http.Native.Manager
{
	[AttributeUsage(AttributeTargets.Method)]
	class RequestHandlerAttribute : Attribute
	{
		public string Path { get; }
		public RequestHeaders.MethodType Method;

		public RequestHandlerAttribute(string path, RequestHeaders.MethodType method)
		{
			Path = path;
			Method = method;
		}
	}
}
