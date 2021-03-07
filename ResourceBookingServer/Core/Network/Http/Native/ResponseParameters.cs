using Core.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Network.Http.Native
{
	class ResponseParameters
	{
		private readonly byte[] responseContent;
		private readonly int statusCode;
		private readonly HttpOperations.ContentType contentType;
		private readonly Dictionary<string, string> headers;

		public ResponseParameters()
		{

		}
		public ResponseParameters(byte[] responseContent, int statusCode, HttpOperations.ContentType contentType, Dictionary<string, string> headers = null)
		{
			this.responseContent = responseContent;
			this.statusCode = statusCode;
			this.contentType = contentType;
			this.headers = headers;
		}

		public List<byte> ToByteList(string serverName)
		{
			if (responseContent == null)
			{
				return null;
			}

			var buffer = CompressionOperations.GetGZipCompressedData(responseContent);
			var responseHeaders = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("Accept-Ranges", "bytes"),
				new Tuple<string, string>("Connection", "Keep-Alive"),
				new Tuple<string, string>("Content-Encoding", "gzip"),
				new Tuple<string, string>("Content-Length", buffer.Length.ToString()),
				new Tuple<string, string>("Content-Type", HttpOperations.contentTypes[contentType]),
				new Tuple<string, string>("Keep-Alive", "timeout=1, max=100"),
				new Tuple<string, string>("Server", serverName),
				new Tuple<string, string>("Vary", "Accept-Encoding,User-Agent"),
			};

			if (headers != null)
			{
				foreach (var header in headers)
				{
					responseHeaders.Add(new Tuple<string, string>(header.Key, header.Value));
				}
			}

			var responseData = $"HTTP/1.1 {statusCode}\r\n" + HttpOperations.GetFormattedHeaders(responseHeaders);
			var response = new List<byte>(Encoding.ASCII.GetBytes(responseData));

			response.AddRange(buffer);

			return response;
		}
	}
}
