using System;
using System.Diagnostics;

namespace Core.Diagnostic
{
	class Timer
	{
		private readonly Stopwatch stopwatch;

		private string GetLogMessage(string messagePrefix, bool restart)
		{
			return $"{messagePrefix}: {GetTimeInSeconds(restart)}";
		}

		public Timer()
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
		}

		public long GetTimeInMilliseconds(bool restart = false)
		{
			var time = stopwatch.ElapsedMilliseconds;

			if (restart)
			{
				Restart();
			}

			return time;
		}
		public double GetTimeInSeconds(bool restart = false)
		{
			return GetTimeInMilliseconds(restart) / 1000.0;
		}

		public void LogTime(string messagePrefix, Logger.LogLevel logLevel = Logger.LogLevel.Debug, bool restart = false)
		{
			Logger.Log("Timer", GetLogMessage(messagePrefix, restart), logLevel);
		}
		public int LogTimeWithPadding(string messagePrefix, int logIndex, Logger.LogLevel logLevel = Logger.LogLevel.Debug, bool restart = false)
		{
			return Logger.LogWithPadding("Timer", GetLogMessage(messagePrefix, restart), logIndex, logLevel);
		}

		public void Restart()
		{
			stopwatch.Restart();
		}

		public static bool CheckWithTime(double time, Func<bool> callback, int sleepTime = 100, Timer timer = null)
		{
			if (timer == null)
			{
				timer = new Timer();
			}

			var timeInMilliseconds = (long)(time * 1000);
			var isTimeReached = false;

			while (true)
			{
				if (callback())
				{
					return true;
				}
				else
				{
					if (timer.GetTimeInMilliseconds() < timeInMilliseconds)
					{
						System.Threading.Thread.Sleep(sleepTime);
					}
					else
					{
						if (isTimeReached)
						{
							return false;
						}
						else
						{
							isTimeReached = true;
						}
					}
				}
			}
		}
	}
}
