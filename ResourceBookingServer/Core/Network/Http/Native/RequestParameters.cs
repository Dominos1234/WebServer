using System;
using System.Collections.Generic;

namespace Core.Network.Http.Native
{
	class RequestParameters
	{
		public string RequestedPath { get; set; }
		public RequestHeaders Headers { get; private set; }
		public PostData PostData { get; private set; }

		public Func<byte[], bool> DataSender { get; }

		public RequestParameters(Func<byte[], bool> dataSender)
		{
			DataSender = dataSender;
		}

		public bool Load(List<byte> data)
		{
			if (Headers == null)
			{
				Headers = RequestHeaders.FromData(data, out var requestedPath);
				RequestedPath = requestedPath;
			}

			if (Headers != null)
			{
				if (data.Count >= Headers.ContentLength)
				{
					if (Headers.ContentLength > 0)
					{
						PostData = new PostData(data.GetRange(0, Headers.ContentLength));
						data.RemoveRange(0, Headers.ContentLength);
					}
					else
					{
						PostData = new PostData();
					}

					return true;
				}
			}

			return false;
		}

		public void Clear()
		{
			Headers = null;
			RequestedPath = null;
			PostData = null;
		}
	}
}
