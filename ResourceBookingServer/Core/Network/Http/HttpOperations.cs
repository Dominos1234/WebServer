using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Network.Http
{
	class HttpOperations
	{
		private static readonly string internalResourcesPath = "Internal\\";
		private static readonly string responseMessage500 = "500 Internal Server Error";

		public static readonly string userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36";

		public enum ContentType
		{
			Html, Css, JavaScript, Text, Jpeg, Json
		}

		public static readonly Dictionary<ContentType, string> contentTypes = new Dictionary<ContentType, string>()
		{
			[ContentType.Html] = "text/html",
			[ContentType.Css] = "text/css",
			[ContentType.JavaScript] = "text/javascript",
			[ContentType.Text] = "text/plain",
			[ContentType.Jpeg] = "image/jpeg",
			[ContentType.Json] = "application/json"
		};

		public static byte[] GetFileContent(string resourcesPath, string requestedPath, bool getInternalPage, out int statusCode, out ContentType contentType)
		{
			var filePath = resourcesPath != null ? resourcesPath + requestedPath : null;

			if (filePath != null && File.Exists(filePath))
			{
				statusCode = 200;

				if (filePath.EndsWith("html"))
				{
					contentType = ContentType.Html;
				}
				else if (filePath.EndsWith("css"))
				{
					contentType = ContentType.Css;
				}
				else if (filePath.EndsWith("js"))
				{
					contentType = ContentType.JavaScript;
				}
				else
				{
					contentType = ContentType.Text;
				}

				return File.ReadAllBytes(filePath);
			}
			else
			{
				if (getInternalPage)
				{
					statusCode = 500;
					contentType = ContentType.Html;

					return Encoding.UTF8.GetBytes(responseMessage500);
				}
				else
				{
					filePath = internalResourcesPath + "404.html";
					return GetFileContent(resourcesPath, filePath, true, out statusCode, out contentType);
				}
			};
		}

		public static string GetFormattedParamters(Dictionary<string, string> parameters)
		{
			var formattedParamters = new List<string>();

			if (parameters != null)
			{
				foreach (var parameter in parameters)
				{
					formattedParamters.Add($"{parameter.Key}={parameter.Value}");
				}
			}

			return string.Join("&", formattedParamters);
		}
		public static string GetFormattedHeaders(IEnumerable<Tuple<string, string>> headers)
		{
			string formattedHeaders = "";

			foreach (var header in headers)
			{
				formattedHeaders += $"{header.Item1}: {header.Item2}\r\n";
			}

			formattedHeaders += "\r\n";

			return formattedHeaders;
		}
	}
}
