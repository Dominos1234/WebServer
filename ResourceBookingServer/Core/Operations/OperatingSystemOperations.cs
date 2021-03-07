using System;
using System.Diagnostics;

namespace Core.Operations
{
	class OperatingSystemOperations
	{
		public static void RestartSystem()
		{
			Process process;

			if (IsWindowsSystem())
			{
				process = ProcessOperations.ExecuteProcess("cmd", "/C shutdown -f -r", createNoWindow: true);
			}
			else
			{
				process = ProcessOperations.ExecuteProcess("sudo", "reboot");
			}

			process.WaitForExit();
		}

		public static bool IsWindowsSystem()
		{
			var platform = (int)Environment.OSVersion.Platform;

			return platform != 4 && platform != 6 && platform != 128;
		}
	}
}
