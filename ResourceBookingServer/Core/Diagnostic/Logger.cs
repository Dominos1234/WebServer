using Core.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Core.Diagnostic
{
	class Logger
	{
		private static readonly string messageFormat = "{0} - {1} - {2}";

		private static readonly Timer timer = new Timer();
		private static readonly object inputDataLockObject = new object();
		private static readonly object logFileLockObject = new object();
		private static readonly object paddingDataLockObject = new object();
		private static readonly object logPositionsLockObject = new object();

		private static readonly Dictionary<int, List<int>> paddingData = new Dictionary<int, List<int>>();
		private static readonly List<Tuple<string, ConsoleColor>> logPositionMessages = new List<Tuple<string, ConsoleColor>>();

		private static string inputData = "";
		private static List<string> inputLines = new List<string>();

		private static string logFolderPath = null;
		private static string logFilePath = null;

		private static bool isReaderInitialized = false;

		public enum LogLevel
		{
			None = -1,
			Debug,
			Info,
			Warning,
			Error
		};

		public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
		public static HashSet<string> SourceBlackList { get; } = new HashSet<string>();
		public static string LogFolderPath
		{
			get => logFolderPath;
			set
			{
				var logFilePath = value + FileOperations.GetFileNameFromDate();

				logFolderPath = value;

				lock (logFileLockObject)
				{
					Logger.logFilePath = logFilePath;
				}
			}
		}
		public static int LogPaddingIndex
		{
			get
			{
				lock (paddingDataLockObject)
				{
					var logIndex = paddingData.Count;
					paddingData.Add(logIndex, new List<int>());

					return logIndex;
				}
			}
		}
		public static int LogPosition
		{
			get
			{
				lock (logPositionsLockObject)
				{
					var logPosition = logPositionMessages.Count;

					logPositionMessages.Add(new Tuple<string, ConsoleColor>("", ConsoleColor.Black));

					lock (inputDataLockObject)
					{
						if (Console.CursorTop + 1 < Console.BufferHeight)
						{
							Console.CursorTop += 1;
						}
					}

					return logPosition;
				}
			}
		}

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		private static bool IsConsoleAttached()
		{
			var platform = (int)Environment.OSVersion.Platform;

			return platform == 4 || platform == 6 || platform == 128 || GetConsoleWindow() != IntPtr.Zero;
		}

		private static void ClearConsoleLastLine()
		{
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, Console.CursorTop - 1);
		}

		private static void ReadAllInput()
		{
			lock (inputDataLockObject)
			{
				var isInputDataCharacterRemoved = false;

				while (Console.KeyAvailable)
				{
					var key = Console.ReadKey().KeyChar;

					if (key == '\r')
					{
						inputLines.Add(inputData);
						inputData = "";

						Console.WriteLine();
					}
					else if (key == '\b')
					{
						if (inputData.Length > 0)
						{
							inputData = inputData.Remove(inputData.Length - 1);
							isInputDataCharacterRemoved = true;
						}
					}
					else
					{
						inputData += key;
					}
				}

				if (isInputDataCharacterRemoved)
				{
					ClearConsoleLastLine();

					Console.Write(inputData);
				}
			}
		}

		private static string GetLogMessage(string source, string message)
		{
			var dateTime = DateTime.UtcNow;
			var fullMessage = string.Format(messageFormat, message, timer.GetTimeInMilliseconds() / 1000.0, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

			return $"{source}::{fullMessage}";
		}

		public static void InitializeReader()
		{
			isReaderInitialized = true;

			new Thread(() =>
			{
				while (true)
				{
					ReadAllInput();

					Thread.Sleep(1);
				}
			}).Start();
		}

		public static void Log(string source, string message, LogLevel logLevel = LogLevel.Debug, int logPosition = -1, bool isFullMessage = false, bool logToFileOnly = false)
		{
			if (logLevel < MinimumLogLevel || SourceBlackList.Contains(source))
			{
				return;
			}

			var logMessage = isFullMessage ? message : GetLogMessage(source, message);

			if (!logToFileOnly)
			{
				ConsoleColor color;

				if (logLevel == LogLevel.Debug)
				{
					color = ConsoleColor.Gray;
				}
				else if (logLevel == LogLevel.Info)
				{
					color = ConsoleColor.White;
				}
				else if (logLevel == LogLevel.Warning)
				{
					color = ConsoleColor.Yellow;
				}
				else
				{
					color = ConsoleColor.Red;
				}

				if (IsConsoleAttached())
				{
					if (isReaderInitialized)
					{
						ReadAllInput();
					}

					lock (inputDataLockObject)
					{
						if (inputData != "")
						{
							ClearConsoleLastLine();
						}

						Console.ForegroundColor = color;

						if (logPosition != -1)
						{
							var previousCursorTop = Console.CursorTop;

							Console.CursorTop = Math.Min(Math.Max(Console.CursorTop - Console.WindowHeight + 1, 0) + logPosition, Console.BufferHeight - 1);
							Console.WriteLine(logMessage);
							Console.CursorTop = previousCursorTop;

							lock (logPositionsLockObject)
							{
								logPositionMessages[logPosition] = new Tuple<string, ConsoleColor>(logMessage, color);
							}
						}
						else
						{
							Console.WriteLine(logMessage);

							lock (logPositionsLockObject)
							{
								if (logPositionMessages.Count > 0)
								{
									if (Console.CursorTop >= Console.WindowHeight)
									{
										var previousCursorTop = Console.CursorTop;

										Console.CursorTop = Console.CursorTop - Console.WindowHeight + 1;

										foreach (var logPositionMessage in logPositionMessages)
										{
											Console.ForegroundColor = logPositionMessage.Item2;
											Console.WriteLine(logPositionMessage.Item1.PadRight(Console.WindowWidth - 1));
										}

										Console.CursorTop = previousCursorTop;
									}
								}
							}
						}

						Console.ResetColor();

						if (inputData != "")
						{
							Console.Write(inputData);
						}
					}
				}
			}

			lock (logFileLockObject)
			{
				if (logFilePath != null)
				{
					File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
				}
			}
		}

		public static int LogWithPadding(string source, string message, int logIndex = -1, LogLevel logLevel = LogLevel.Debug, int logPosition = -1)
		{
			var messageParts = GetLogMessage(source, message).Split(' ');
			var messageWithPadding = "";

			lock (paddingDataLockObject)
			{
				if (logIndex < 0)
				{
					logIndex = paddingData.Count;
					paddingData.Add(logIndex, new List<int>());
				}

				var paddingDataRecord = paddingData[logIndex];

				for (var index = 0; index < messageParts.Length; ++index)
				{
					if (index < paddingDataRecord.Count)
					{
						if (paddingDataRecord[index] <= messageParts[index].Length)
						{
							paddingDataRecord[index] = messageParts[index].Length;
							messageWithPadding += messageParts[index];
						}
						else
						{
							messageWithPadding += messageParts[index] + new string(' ', paddingDataRecord[index] - messageParts[index].Length);
						}
					}
					else
					{
						paddingDataRecord.Add(messageParts[index].Length);
						messageWithPadding += messageParts[index];
					}

					if (index + 1 != messageParts.Length)
					{
						messageWithPadding += ' ';
					}
				}
			}

			Log(source, messageWithPadding, logLevel, logPosition, true);

			return logIndex;
		}

		public static string ReadLine()
		{
			lock (inputDataLockObject)
			{
				if (inputLines.Count > 0)
				{
					var line = inputLines[0];

					inputLines.RemoveAt(0);

					return line;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
