using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Core.Network.SocketDataProcessor
{
	abstract class SocketBaseDataProcessor
	{
		private readonly object receivedDataLock = new object();

		private List<byte> receivedData;

		private ManualResetEvent receivedDataResetEvent;
		private Diagnostic.Timer lastReceivedDataTimer;

		protected abstract int MaxTimeWithoutDataTillTimeoutInMilliseconds { get; }

		public string ClientIPAddress { get; private set; }

		public Action<byte[], int> OnDataSendCallback { private get; set; }
		public Action OnCloseConnectionCallback { private get; set; }

		public bool IsConnectionOpened { get; private set; }
		public bool IsMaxTimeWithoutDataTillTimeoutExceeded
		{
			get
			{
				return MaxTimeWithoutDataTillTimeoutInMilliseconds > 0 && (int)lastReceivedDataTimer.GetTimeInMilliseconds() > MaxTimeWithoutDataTillTimeoutInMilliseconds;
			}
		}

		protected abstract bool OnDataReadCallback(List<byte> buffer);

		protected virtual void SendData(byte[] data, int dataLength = 0)
		{
			if (dataLength == 0)
			{
				dataLength = data.Length;
			}

			OnDataSendCallback(data, dataLength);
		}
		protected virtual void SendData(List<byte> data)
		{
			SendData(data.ToArray());
		}
		protected virtual void SendData(string data)
		{
			SendData(Encoding.ASCII.GetBytes(data));
		}

		protected void CloseSocket()
		{
			OnCloseConnectionCallback?.Invoke();
		}

		public virtual void OnConnectionOpened(string clientIPAddress)
		{
			lastReceivedDataTimer = new Diagnostic.Timer();
			receivedData = new List<byte>();
			receivedDataResetEvent = new ManualResetEvent(false);

			ClientIPAddress = clientIPAddress;
			IsConnectionOpened = true;

			new Thread(() =>
			{
				while (IsConnectionOpened)
				{
					List<byte> currentReceivedData;

					receivedDataResetEvent.WaitOne();

					lock (receivedDataLock)
					{
						currentReceivedData = receivedData;
						receivedData = new List<byte>();
						receivedDataResetEvent.Reset();
					}

					if (currentReceivedData.Count > 0)
					{
						if (!OnDataReadCallback(currentReceivedData))
						{
							CloseSocket();
						}
					}
				}
			}).Start();
		}

		public virtual void OnConnectionClosed()
		{
			IsConnectionOpened = false;

			lock (receivedDataLock)
			{
				receivedDataResetEvent.Set();
			}
		}

		public void OnPureDataReadCallback(byte[] buffer)
		{
			lastReceivedDataTimer.Restart();

			lock (receivedDataLock)
			{
				receivedData.AddRange(buffer);
				receivedDataResetEvent.Set();
			}
		}
	}
}
