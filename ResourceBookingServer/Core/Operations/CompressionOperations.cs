using System.IO;
using System.IO.Compression;

namespace Core.Operations
{
	class CompressionOperations
	{
		public static byte[] GetGZipCompressedData(byte[] buffer)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
				{
					compressionStream.Write(buffer, 0, buffer.Length);
				}

				return memoryStream.ToArray();
			}
		}

		public static byte[] GetGZipDecompressedData(byte[] buffer)
		{
			using (var memoryStream = new MemoryStream(buffer))
			{
				using (var decompressionStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					using (var decompressedMemoryStream = new MemoryStream())
					{
						decompressionStream.CopyTo(decompressedMemoryStream);

						return decompressedMemoryStream.ToArray();
					}
				}
			}
		}
	}
}
