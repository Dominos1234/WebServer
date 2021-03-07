using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Operations
{
	class FileOperations
	{
		private static readonly int timestampMaxLength = 13;

		public static void RemoveFilesInDirectory(string path, string[] fileNamesToPreserve = null)
		{
			var files = Directory.GetFiles(path);

			foreach (var filePath in files)
			{
				var removeFile = true;

				if (fileNamesToPreserve != null)
				{
					var fileName = Path.GetFileName(filePath);

					removeFile = !fileNamesToPreserve.Contains(fileName);
				}

				if (removeFile)
				{
					File.Delete(filePath);
				}
			}
		}

		public static List<string> GetFilesInDirectory(string path, string prefix = "", string suffix = "", string searchPattern = null, bool getFullPath = true, bool removeSuffix = true, bool getFilesInSubDirectories = false)
		{
			var result = new List<string>();

			if (Directory.Exists(path))
			{
				if (searchPattern == null)
				{
					searchPattern = "*";
				}

				var files = Directory.GetFiles(path, searchPattern);

				foreach (var filePath in files)
				{
					var isCorrectFile = true;

					if (prefix != "")
					{
						var fileName = Path.GetFileName(filePath);

						isCorrectFile = fileName.StartsWith(prefix);
					}

					if (isCorrectFile && filePath.EndsWith(suffix))
					{
						string preparedFilePath;

						if (removeSuffix && suffix != "")
						{
							preparedFilePath = filePath.Remove(filePath.IndexOf(suffix));
						}
						else
						{
							preparedFilePath = filePath;
						}

						if (getFullPath)
						{
							result.Add(preparedFilePath);
						}
						else
						{
							result.Add(Path.GetFileName(preparedFilePath));
						}
					}
				}

				if (getFilesInSubDirectories)
				{
					var directories = Directory.GetDirectories(path);

					foreach (var directoryPath in directories)
					{
						result.AddRange(GetFilesInDirectory(directoryPath, prefix, suffix, searchPattern, getFullPath, removeSuffix, getFilesInSubDirectories));
					}
				}
			}

			return result;
		}
		public static List<string> GetFilesInDirectory(string path, string[] suffixData, string searchPattern = null, bool getFullPath = true, bool removeSuffix = true)
		{
			var result = new List<string>();

			foreach (var suffix in suffixData)
			{
				result.AddRange(GetFilesInDirectory(path, "", suffix, searchPattern, getFullPath, removeSuffix));
			}

			return result;
		}

		public static string GetFileNameFromDate(DateTime? dateTime = null, bool getFullDate = false)
		{
			if (dateTime == null)
			{
				dateTime = DateTime.UtcNow;
			}

			if (getFullDate)
			{
				return dateTime.Value.ToString("yyyy-MM-dd HH-mm-ss ---FFFFFFF");
			}
			else
			{
				return dateTime.Value.ToString("yyyy-MM-dd");
			}
		}

		public static string GetGeneratedPathFromName(string name, int nameMaxLength, int groupNameLength = 3)
		{
			var path = "";
			var paddedName = name.PadLeft(nameMaxLength, '0');
			var groupName = "";

			var firstGroupLength = paddedName.Length % groupNameLength;

			if (firstGroupLength > 0)
			{
				path += $"{paddedName.Substring(0, firstGroupLength)}/";
			}

			for (var index = firstGroupLength; index < paddedName.Length; ++index)
			{
				groupName += paddedName[index];

				if (groupName.Length == groupNameLength)
				{
					path += $"{groupName}/";

					groupName = "";
				}
			}

			return path;
		}
		public static string GetGeneratedPathFromTimestamp(ulong timestamp, int differendPathsInSecond)
		{
			var timestampFragmentToRemoveLength = 6 - (int)Math.Ceiling(Math.Log10(1000.0 / differendPathsInSecond));
			var timestampString = timestamp.ToString();

			return GetGeneratedPathFromName(timestampString.Substring(0, timestampString.Length - timestampFragmentToRemoveLength), timestampMaxLength - timestampFragmentToRemoveLength);
		}

		public static string GetFileNameWithoutExtension(string fileName, out string extension)
		{
			var fileNameParts = new List<string>(fileName.Split('.'));

			extension = fileNameParts[fileNameParts.Count - 1];

			fileNameParts.RemoveAt(fileNameParts.Count - 1);

			return string.Join(".", fileNameParts);
		}

		public static byte[] ReadBytesFromFile(string path, int offset = 0, int length = 0)
		{
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				if (length == 0)
				{
					length = (int)fileStream.Length - offset;
				}

				byte[] data = new byte[length];

				fileStream.Seek(offset, SeekOrigin.Begin);
				fileStream.Read(data, 0, length);

				return data;
			}
		}
		public static byte[] ReadLastBytesFromFile(string path, int length)
		{
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				if (fileStream.Length < length)
				{
					length = (int)fileStream.Length;
				}

				byte[] data = new byte[length];

				fileStream.Seek(-length, SeekOrigin.End);
				fileStream.Read(data, 0, length);

				return data;
			}
		}

		public static void WriteAllBytes(string path, byte[] bytes, bool createDirectory = true)
		{
			if (createDirectory)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			File.WriteAllBytes(path, bytes);
		}

		public static bool IsCorrectFileName(string fileName)
		{
			return !fileName.Contains(".") && !fileName.Contains("/") && !fileName.Contains(@"\");
		}
	}
}
