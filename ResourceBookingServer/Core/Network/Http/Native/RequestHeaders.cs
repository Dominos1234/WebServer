using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Network.Http.Native
{
	class RequestHeaders
	{
		public enum MethodType
		{
			Get, Post, Delete, Invalid
		}

		private static readonly string separator = "\r\n\r\n";

		public MethodType Method { get; }
		public int ContentLength { get; }
		public Dictionary<string, string> Data;

		public RequestHeaders(string method, string[] headers)
		{
			Data = new Dictionary<string, string>();

			if (method == "get")
			{
				Method = MethodType.Get;
			}
			else if (method == "post")
			{
				Method = MethodType.Post;
			}
			else if (method == "delete")
			{
				Method = MethodType.Delete;
			}
			else
			{
				Method = MethodType.Invalid;
			}

			foreach (var header in headers)
			{
				var parts = header.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 2)
				{
					if (parts[0].ToLower() == "content-length" && int.TryParse(parts[1], out var contentLength))
					{
						ContentLength = contentLength;
					}
					else
					{
						Data[parts[0]] = parts[1];
					}
				}
			}
		}

		public static RequestHeaders FromData(List<byte> data, out string requestedPath)
		{
			var text = Encoding.ASCII.GetString(data.ToArray());
			var separatorIndex = text.IndexOf(separator);

			if (separatorIndex > 0)
			{
				var parts = text.Split(new string[] { separator }, 2, StringSplitOptions.None);
				var headers = parts[0].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

				if (headers.Length > 0)
				{
					var requestPathParts = headers[0].Split(' ');

					text = parts[1];

					data.RemoveRange(0, separatorIndex + separator.Length);

					if (requestPathParts.Length == 3)
					{
						var method = requestPathParts[0].ToLower();

						requestedPath = requestPathParts[1];

						return new RequestHeaders(method, headers);
					}
				}
			}

			requestedPath = null;

			return null;
		}
	}
}
