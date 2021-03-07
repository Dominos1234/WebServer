using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Operations
{
	class StandardDataOperations
	{
		public static T[] GetSubArray<T>(T[] data, int index, int length = 0)
		{
			if (length == 0)
			{
				length = data.Length - index;
			}

			var result = new T[length];

			Array.Copy(data, index, result, 0, length);

			return result;
		}
		public static T[][] GetSubArray<T>(T[][] data, int x, int y, int width, int height)
		{
			var result = new T[width][];

			for (var index = 0; index < width; ++index)
			{
				result[index] = GetSubArray(data[x + index], y, height);
			}

			return result;
		}
		public static T[,] GetSubArray<T>(T[,] data, int x, int y, int width, int height)
		{
			var result = new T[width, height];

			for (var positionX = 0; positionX < width; ++positionX)
			{
				for (var positionY = 0; positionY < height; ++positionY)
				{
					result[positionX, positionY] = data[positionX + x, positionY + y];
				}
			}

			return result;
		}

		public static byte[] RemoveValueFromEnd(byte[] data, byte valueToRemove)
		{
			for (int index = data.Length - 1; index >= 0; --index)
			{
				if (!data[index].Equals(valueToRemove))
				{
					return GetSubArray(data, 0, index + 1);
				}
			}

			return new byte[0];
		}

		public static List<byte[]> Split(byte[] data, byte separator, int maxLength = -1)
		{
			List<byte[]> separatedData = new List<byte[]>();
			int startIndex = 0;

			for (int index = 0; index < data.Length; ++index)
			{
				if (data[index] == separator)
				{
					separatedData.Add(GetSubArray(data, startIndex, index - startIndex));
					startIndex = index + 1;

					if (separatedData.Count == maxLength - 1)
					{
						break;
					}
				}
			}

			if (startIndex != data.Length)
			{
				separatedData.Add(GetSubArray(data, startIndex, data.Length - startIndex));
			}

			return separatedData;
		}
		public static List<byte[]> Split(byte[] data, byte[] separator, int maxLength = -1)
		{
			List<byte[]> separatedData = new List<byte[]>();
			int startIndex = 0;

			for (int index = 0; index < data.Length - separator.Length; ++index)
			{
				var isSeparatorFounded = true;

				for (var separatorIndex = 0; separatorIndex < separator.Length; ++separatorIndex)
				{
					if (data[index + separatorIndex] != separator[separatorIndex])
					{
						isSeparatorFounded = false;
						break;
					}
				}

				if (isSeparatorFounded)
				{
					separatedData.Add(GetSubArray(data, startIndex, index - startIndex));
					startIndex = index + separator.Length;

					if (separatedData.Count == maxLength - 1)
					{
						break;
					}
				}
			}

			if (startIndex != data.Length)
			{
				separatedData.Add(GetSubArray(data, startIndex, data.Length - startIndex));
			}

			return separatedData;
		}

		public static bool StartsWith(byte[] data, byte[] startWithData)
		{
			if (data.Length >= startWithData.Length)
			{
				for (var index = 0; index < startWithData.Length; ++index)
				{
					if (data[index] != startWithData[index])
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
		public static bool EndsWith(byte[] data, byte[] startWithData)
		{
			if (data.Length >= startWithData.Length)
			{
				var dataStartIndex = data.Length - startWithData.Length;

				for (var index = 0; index < startWithData.Length; ++index)
				{
					if (data[dataStartIndex + index] != startWithData[index])
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool AreArraysEqual(byte[] array, byte[] pattern, int arrayOffset = 0, int arrayLength = 0)
		{
			if (arrayLength == 0)
			{
				arrayLength = array.Length;
			}

			if (pattern.Length == arrayLength)
			{
				for (var index = 0; index < pattern.Length; ++index)
				{
					if (array[arrayOffset + index] != pattern[index])
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
		public static bool AreKeysInDictionary<K, V>(Dictionary<K, V> dictionary, IList<K> keys)
		{
			if (dictionary == null)
			{
				return false;
			}

			foreach (var key in keys)
			{
				if (!dictionary.ContainsKey(key))
				{
					return false;
				}
			}

			return true;
		}
		public static bool AreNotNullKeysInDictionary<K, V>(Dictionary<K, V> dictionary, IList<K> keys)
		{
			if (dictionary == null)
			{
				return false;
			}

			foreach (var key in keys)
			{
				if (!dictionary.ContainsKey(key) || dictionary[key] == null)
				{
					return false;
				}
			}

			return true;
		}

		public static ulong ReadUInt64(byte[] buffer, int offset = 0, int length = 0)
		{
			UInt64 value = 0;

			if (length == 0)
			{
				length = buffer.Length - offset;
			}

			for (var index = 0; index < length; ++index)
			{
				value = value * 256 + buffer[index + offset];
			}

			return value;
		}
		public static ulong ReadUInt64(List<byte> buffer, int offset = 0, int length = 0)
		{
			UInt64 value = 0;

			if (length == 0)
			{
				length = buffer.Count - offset;
			}

			for (var index = 0; index < length; ++index)
			{
				value = value * 256 + buffer[index + offset];
			}

			return value;
		}
		public static int ReadInt(List<byte> buffer, int offset = 0, int length = 0)
		{
			var value = 0;
			var isMinusSign = false;

			if (length == 0)
			{
				length = buffer.Count - offset;
			}

			if ((buffer[offset] & 0x80) > 0)
			{
				buffer[offset] &= 0x7F;
				isMinusSign = true;
			}

			for (var index = 0; index < length; ++index)
			{
				value = value * 256 + buffer[index + offset];
			}

			if (isMinusSign)
			{
				value *= -1;
			}

			return value;
		}
		public static double ReadDouble(List<byte> buffer, int offset = 0)
		{
			var value = ReadUInt64(buffer, offset, 8);

			return BitConverter.Int64BitsToDouble((long)value);
		}
		public static string ReadString(byte[] bufferArray, ref int offset, int dataSizeLength = 2)
		{
			if (offset + dataSizeLength < bufferArray.Length)
			{
				var stringLength = (int)ReadUInt64(bufferArray, offset, dataSizeLength);

				offset += dataSizeLength;

				if (offset + stringLength <= bufferArray.Length)
				{
					var value = Encoding.ASCII.GetString(bufferArray, offset, stringLength);

					offset += stringLength;

					return value;
				}
			}

			return null;
		}
		public static string ReadString(List<byte> buffer, ref int offset, int dataSizeLength = 2)
		{
			return ReadString(buffer.ToArray(), ref offset, dataSizeLength);
		}
		public static string ReadString(List<byte> buffer, int offset, int dataSizeLength = 2)
		{
			return ReadString(buffer, ref offset, dataSizeLength);
		}
		public static List<string> ReadStringArray(List<byte> buffer, int offset = 0, int dataElementSize = 2)
		{
			var values = new List<string>();
			var bufferArray = buffer.ToArray();

			while (true)
			{
				var value = ReadString(bufferArray, ref offset, dataElementSize);

				if (value != null)
				{
					values.Add(value);
				}
				else
				{
					break;
				}
			}

			return values;
		}

		public static void WriteData(List<byte> buffer, ulong dataToWrite, int dataSize)
		{
			for (int index = dataSize - 1; index >= 0; --index)
			{
				buffer.Add((byte)((dataToWrite >> (index * 8)) & 0xff));
			}
		}
		public static void WriteData(List<byte> buffer, int dataToWrite, int dataSize)
		{
			bool addLessThanZero = dataToWrite < 0;

			if (addLessThanZero)
			{
				dataToWrite *= -1;
			}

			WriteData(buffer, (ulong)dataToWrite, dataSize);

			if (addLessThanZero)
			{
				buffer[buffer.Count - dataSize] |= 128;
			}
		}
		public static void WriteData(List<byte> buffer, double dataToWrite)
		{
			var value = BitConverter.DoubleToInt64Bits(dataToWrite);

			WriteData(buffer, (ulong)value, 8);
		}
		public static void WriteData(List<byte> buffer, string dataToWrite, int dataElementSize = 2)
		{
			WriteData(buffer, (ulong)dataToWrite.Length, dataElementSize);

			buffer.AddRange(Encoding.ASCII.GetBytes(dataToWrite));
		}
		public static void WriteData(List<byte> buffer, List<string> dataToWrite, int dataElementSize = 2)
		{
			foreach (var elementToWrite in dataToWrite)
			{
				WriteData(buffer, elementToWrite, dataElementSize);
			}
		}
		public static List<byte> WriteData(int dataToWrite, int dataSize)
		{
			var buffer = new List<byte>();

			WriteData(buffer, dataToWrite, dataSize);

			return buffer;
		}
		public static List<byte> WriteData(ulong dataToWrite, int dataSize)
		{
			var buffer = new List<byte>();

			WriteData(buffer, dataToWrite, dataSize);

			return buffer;
		}

		public static string GetHexStringFromByteArray(byte[] buffer)
		{
			StringBuilder stringBuilder = new StringBuilder(buffer.Length * 2);

			foreach (byte value in buffer)
			{
				stringBuilder.AppendFormat("{0:x2}", value);
			}
			
			return stringBuilder.ToString();
		}

		public static V GetValueFromDictionary<T, V>(Dictionary<T, V> dictionary, T key, V defaultValue)
		{
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}
			else
			{
				return defaultValue;
			}
		}

		public static Dictionary<T, V> MapArrayToDictionary<T, V>(IEnumerable<V> array, Func<V, T> mapper)
		{
			var dictionary = new Dictionary<T, V>();

			foreach (var value in array)
			{
				var key = mapper(value);

				dictionary[key] = value;
			}

			return dictionary;
		}
		public static Dictionary<T, List<V>> MapArrayToListDictionary<T, V>(IEnumerable<V> array, Func<V, T> mapper)
		{
			var dictionary = new Dictionary<T, List<V>>();

			foreach (var value in array)
			{
				var key = mapper(value);

				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = new List<V>();
				}

				dictionary[key].Add(value);
			}

			return dictionary;
		}

		public static List<T> GetCastedValue<T>(List<dynamic> values)
		{
			var list = new List<T>();

			foreach (var value in values)
			{
				list.Add(value);
			}

			return list;
		}
		public static List<List<T>> GetCastedValue2D<T>(List<dynamic> values)
		{
			var list = new List<List<T>>();

			foreach (var value in values)
			{
				list.Add(GetCastedValue<T>(value));
			}

			return list;
		}

		public static void SwapData<T>(ref T first, ref T second)
		{
			T tmp = first;

			first = second;
			second = tmp;
		}

		public static void WriteListToArray<T>(T[] array, List<T> list, int offset)
		{
			Array.Copy(list.ToArray(), 0, array, offset, list.Count);
		}

		public static Dictionary<string, int> TryParseIntArray(string[] keys, string[] dataToParse, int offset = 0)
		{
			if (dataToParse.Length >= keys.Length + offset)
			{
				Dictionary<string, int> parsedData = new Dictionary<string, int>();

				for (var index = 0; index < keys.Length; ++index)
				{
					if (int.TryParse(dataToParse[index + offset], out var parsedValue))
					{
						parsedData[keys[index]] = parsedValue;
					}
					else
					{
						return null;
					}
				}

				return parsedData;
			}
			else
			{
				return null;
			}
		}

		public static List<T> DeepCopy<T>(List<T> source) where T : ICloneable
		{
			return source.Select(item => (T)item.Clone()).ToList();
		}

		public static List<T> GetEnumValues<T>()
		{
			var enumValues = new List<T>();
			var values = Enum.GetValues(typeof(T));

			foreach (var value in values)
			{
				enumValues.Add((T)value);
			}

			return enumValues;
		}
	}
}
