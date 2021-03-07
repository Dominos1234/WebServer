using Core.Diagnostic;
using Core.Network.SocketDataProcessor;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Network.SocketServer
{
	class TcpSocketServer
	{
		private static readonly int bufferLength = 16 * 1024 * 1024;

		private readonly object clientsLockObject = new object();
		private readonly object writerLock = new object();

		private readonly TcpListener listener;
		private readonly List<Tuple<TcpClient, NetworkStream, SocketBaseDataProcessor>> clients = new List<Tuple<TcpClient, NetworkStream, SocketBaseDataProcessor>>();

		private bool isRunning;

		private void CloseClient(Tuple<TcpClient, NetworkStream, SocketBaseDataProcessor> client)
		{
			bool callOnConnectionClose;

			lock (clientsLockObject)
			{
				callOnConnectionClose = clients.Remove(client);
			}

			if (callOnConnectionClose)
			{
				Logger.Log("TcpSocketServer", "CloseClient", Logger.LogLevel.Info);

				Task.Run(() => client.Item3.OnConnectionClosed());
				client.Item1.Close();
			}
		}

		public TcpSocketServer(Func<SocketBaseDataProcessor> socketDataProcessorProvider, int port, bool isAsynchronous = false, string localAddress = null)
		{
			var ipAddress = IPAddress.Parse(localAddress ?? "0.0.0.0");

			isRunning = true;
			listener = new TcpListener(ipAddress, port);
			listener.Start();

			new Thread(() =>
			{
				var buffer = new byte[bufferLength];

				while (isRunning)
				{
					var isAnyDataRead = false;
					List<Tuple<TcpClient, NetworkStream, SocketBaseDataProcessor>> currentClients;

					lock (clientsLockObject)
					{
						currentClients = new List<Tuple<TcpClient, NetworkStream, SocketBaseDataProcessor>>(clients);
					}

					foreach (var client in currentClients)
					{
						try
						{
							if (client.Item2.DataAvailable)
							{
								var amount = client.Item2.Read(buffer, 0, bufferLength);
								var receivedData = new byte[amount];

								Array.Copy(buffer, receivedData, amount);

								client.Item3.OnPureDataReadCallback(receivedData);
								isAnyDataRead = true;
							}
							else
							{
								if (client.Item3.IsMaxTimeWithoutDataTillTimeoutExceeded)
								{
									Logger.Log("TcpSocketServer", "CloseClient - client.Item3.LastReceivedDataTimeInMiliseconds > timeWithoutDataTillTimeout", Logger.LogLevel.Info);
									CloseClient(client);
								}
							}
						}
						catch (Exception exception)
						{
							Logger.Log("TcpSocketServer", exception.ToString(), Logger.LogLevel.Error);
							CloseClient(client);
						}
					}

					if (!isAnyDataRead)
					{
						Thread.Sleep(1);
					}
				}
			}).Start();

			void incomingClientsListener()
			{
				while (isRunning)
				{
					try
					{
						var tcpClient = listener.AcceptTcpClient();
						var clientIPAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
						var socketDataProcessor = socketDataProcessorProvider();
						var networkStream = tcpClient.GetStream();
						var client = new Tuple<TcpClient, NetworkStream, SocketBaseDataProcessor>(tcpClient, networkStream, socketDataProcessor);

						socketDataProcessor.OnDataSendCallback = (byte[] data, int dataLength) =>
						{
							lock (writerLock)
							{
								try
								{
									networkStream.Write(data, 0, dataLength);
								}
								catch (Exception exception)
								{
									Logger.Log("TcpSocketServer", exception.ToString(), Logger.LogLevel.Error);
									CloseClient(client);
								}
							}
						};
						socketDataProcessor.OnCloseConnectionCallback = () =>
						{
							Logger.Log("TcpSocketServer", "CloseClient - onCloseConnectionCallback call", Logger.LogLevel.Info);
							CloseClient(client);
						};

						lock (clientsLockObject)
						{
							clients.Add(client);
							socketDataProcessor.OnConnectionOpened(clientIPAddress);
						}
					}
					catch (Exception exception)
					{
						Logger.Log("TcpSocketServer", exception.ToString(), Logger.LogLevel.Error);
					}
				}
			}

			if (isAsynchronous)
			{
				new Thread(incomingClientsListener).Start();
			}
			else
			{
				incomingClientsListener();
			}
		}

		public void Exit()
		{
			listener.Stop();
			isRunning = false;

			lock (clientsLockObject)
			{
				foreach (var client in clients)
				{
					client.Item3.OnConnectionClosed();
				}
			}
		}
	}
}
