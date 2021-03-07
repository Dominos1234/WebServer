using System;
using System.Collections.Generic;

namespace Core.Operations
{
	class MathOperations
	{
		public static int GetNearestPowerOf2(uint x)
		{
			x--;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			x++;

			return (int)x;
		}

		public static int Log2(int x)
		{
			var r = 0xFFFF - x >> 31 & 0x10;
			x >>= r;
			var shift = 0xFF - x >> 31 & 0x8;
			x >>= shift;
			r |= shift;
			shift = 0xF - x >> 31 & 0x4;
			x >>= shift;
			r |= shift;
			shift = 0x3 - x >> 31 & 0x2;
			x >>= shift;
			r |= shift;
			r |= (x >> 1);

			return r;
		}

		public static int GetBase64InputLength(int base64Length)
		{
			return (base64Length + 3) / 4 * 3;
		}

		public static double GetDegreeFromRadians(double angle)
		{
			return angle * 180.0 / Math.PI;
		}
		public static double GetRadiansFromDegree(double angle)
		{
			return angle * Math.PI / 180.0;
		}

		public static double GetAnglesDelta(double a, double b)
		{
			return 180 - Math.Abs(Math.Abs(a - b) - 180);
		}

		public static bool GetBitAtPosition(byte value, int position)
		{
			return (value & (1 << position)) != 0;
		}

		public static byte GetSubByte(byte value, int startBitIndex, int bitsLength)
		{
			var leftShifted = (byte)(value << (8 - (startBitIndex + bitsLength)) & 0xFF);

			return (byte)(leftShifted >> (8 - bitsLength) & 0xFF);
		}

		public static void SetMinMax<T>(T value, ref T min, ref T max, bool isMinMaxSet)
		{
			var x = Comparer<T>.Default.Compare(value, min);
			var y = Comparer<T>.Default.Compare(value, min);

			if (!isMinMaxSet || Comparer<T>.Default.Compare(value, min) < 0)
			{
				min = value;
			}

			if (!isMinMaxSet || Comparer<T>.Default.Compare(value, max) > 0)
			{
				max = value;
			}
		}

		public static List<int> GetMinimizedData(List<int> data)
		{
			var calculationData = new List<Tuple<int, int>>(data.Count);
			var minimizedData = new List<int>(new int[data.Count]);

			for (var index = 0; index < data.Count; ++index)
			{
				calculationData.Add(new Tuple<int, int>(index, data[index]));
			}

			calculationData.Sort((first, second) =>
			{
				return first.Item2.CompareTo(second.Item2);
			});

			for (var index = 0; index < calculationData.Count; ++index)
			{
				minimizedData[calculationData[index].Item1] = index;
			}

			return minimizedData;
		}

		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
		{
			if (value.CompareTo(min) < 0)
			{
				return min;
			}
			else if (value.CompareTo(max) > 0)
			{
				return max;
			}	
			else
			{
				return value;
			}
		}
	}
}
