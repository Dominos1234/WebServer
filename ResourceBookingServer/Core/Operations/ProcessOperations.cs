using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Core.Operations
{
	class ProcessOperations
	{
		public static string GetResultFromProgram(string fileName, string arguments, bool getStandardError = false, bool createNoWindow = false)
		{
			string result;

			var process = new Process();
			process.StartInfo.FileName = fileName;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = !getStandardError;
			process.StartInfo.RedirectStandardError = getStandardError;
			process.StartInfo.CreateNoWindow = createNoWindow;
			process.Start();

			if (getStandardError)
			{
				result = process.StandardError.ReadToEnd();
			}
			else
			{
				result = process.StandardOutput.ReadToEnd();
			}

			process.WaitForExit();

			return result;

		}

		public static Process ExecuteProcess(string fileName, string arguments = "", bool startMaximized = false, bool createNoWindow = false, bool redirectStandardInput = false, bool redirectStandardOutput = false, bool redirectStandardError = false)
		{
			var process = new Process();

			process.StartInfo.FileName = fileName;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardInput = redirectStandardInput;
			process.StartInfo.RedirectStandardOutput = redirectStandardOutput;
			process.StartInfo.RedirectStandardError = redirectStandardError;
			process.StartInfo.CreateNoWindow = createNoWindow;

			if (startMaximized)
			{
				process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
			}

			try
			{
				process.Start();
				return process;
			}
			catch (Win32Exception)
			{
				return null;
			}
		}

		public static void KillProcess(string processName, bool waitForKill = true)
		{
			var processes = Process.GetProcesses();
			
			foreach (var process in processes)
			{
				if (process.ProcessName == processName)
				{
					try
					{
						process.Kill();

						if (waitForKill)
						{
							process.WaitForExit();
						}
					}
					catch (Exception exception) when (exception is Win32Exception || exception is InvalidOperationException)
					{

					}
				}
			}
		}
		public static void KillProcess(string[] processNames)
		{
			foreach (var processName in processNames)
			{
				KillProcess(processName);
			}
		}
	}
}