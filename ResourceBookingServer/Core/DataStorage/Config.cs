using Core.Operations;
using Core.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.DataStorage
{
	class Config
	{
		private readonly Dictionary<string, string> stringData;
		private readonly Dictionary<string, int> integerData;
		private readonly Dictionary<string, double> doubleData;
		private readonly Dictionary<string, bool> boolData;
		private readonly Dictionary<string, decimal> decimalData;

		private readonly Dictionary<string, List<string>> stringListData;
		private readonly Dictionary<string, List<int>> integerListData;
		private readonly Dictionary<string, List<float>> floatListData;
		private readonly Dictionary<string, List<double>> doubleListData;

		private readonly Dictionary<string, List<List<int>>> integer2DListData;

		private string ToString<T>(Dictionary<string, T> data)
		{
			var stringValue = "";

			foreach (var record in data)
			{
				stringValue += $"{record.Key}: {record.Value}{Environment.NewLine}";
			}

			return stringValue;
		}

		public Config()
		{
			stringData = new Dictionary<string, string>();
			integerData = new Dictionary<string, int>();
			doubleData = new Dictionary<string, double>();
			boolData = new Dictionary<string, bool>();
			decimalData = new Dictionary<string, decimal>();

			stringListData = new Dictionary<string, List<string>>();
			integerListData = new Dictionary<string, List<int>>();
			floatListData = new Dictionary<string, List<float>>();
			doubleListData = new Dictionary<string, List<double>>();

			integer2DListData = new Dictionary<string, List<List<int>>>();
		}
		public Config(string path) : this()
		{
			if (File.Exists(path))
			{
				var lines = File.ReadAllLines(path);

				foreach (var line in lines)
				{
					var data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

					if (data.Length >= 2)
					{
						var textInBrackets = TextOperations.GetSeparatedTextInBrackets(data[1], ", ", out var dimensionsAmount);
						dynamic castedData;
						Type valueType;

						if (data.Length == 3)
						{
							castedData = TextOperations.GetCastedText(textInBrackets, data[2], out valueType);
						}
						else
						{
							castedData = textInBrackets;
							valueType = typeof(string);
						}

						if (dimensionsAmount == 0)
						{
							if (valueType.Name == typeof(string).Name)
							{
								stringData[data[0]] = castedData;
							}
							else if (valueType.Name == typeof(int).Name)
							{
								integerData[data[0]] = castedData;
							}
							else if (valueType.Name == typeof(double).Name)
							{
								doubleData[data[0]] = castedData;
							}
							else if (valueType.Name == typeof(bool).Name)
							{
								boolData[data[0]] = castedData;
							}
							else if (valueType.Name == typeof(decimal).Name)
							{
								decimalData[data[0]] = castedData;
							}
						}
						else if (dimensionsAmount == 1)
						{
							if (valueType.Name == typeof(string).Name)
							{
								stringListData[data[0]] = castedData;
							}
							else if (valueType.Name == typeof(int).Name)
							{
								integerListData[data[0]] = StandardDataOperations.GetCastedValue<int>(castedData);
							}
							else if (valueType.Name == typeof(float).Name)
							{
								floatListData[data[0]] = StandardDataOperations.GetCastedValue<float>(castedData);
							}
							else if (valueType.Name == typeof(double).Name)
							{
								doubleListData[data[0]] = StandardDataOperations.GetCastedValue<double>(castedData);
							}
						}
						else if (dimensionsAmount == 2)
						{
							if (valueType.Name == typeof(int).Name)
							{
								integer2DListData[data[0]] = StandardDataOperations.GetCastedValue2D<int>(castedData);
							}
						}
					}
				}
			}
		}

		public string GetStringData(string name, string defaultValue = "")
		{
			return StandardDataOperations.GetValueFromDictionary(stringData, name, defaultValue);
		}
		public int GetIntegerData(string name, int defaultValue = 0)
		{
			return StandardDataOperations.GetValueFromDictionary(integerData, name, defaultValue);
		}
		public double GetDoubleData(string name, double defaultValue = 0.0)
		{
			return StandardDataOperations.GetValueFromDictionary(doubleData, name, defaultValue);
		}
		public bool GetBoolData(string name, bool defaultValue = false)
		{
			return StandardDataOperations.GetValueFromDictionary(boolData, name, defaultValue);
		}
		public decimal GetDecimalData(string name, decimal defaultValue = 0.0m)
		{
			return StandardDataOperations.GetValueFromDictionary(decimalData, name, defaultValue);
		}

		public List<string> GetStringListData(string name)
		{
			return StandardDataOperations.GetValueFromDictionary(stringListData, name, new List<string>());
		}
		public List<int> GetIntegerListData(string name)
		{
			return StandardDataOperations.GetValueFromDictionary(integerListData, name, new List<int>());
		}
		public List<float> GetFloatListData(string name)
		{
			return StandardDataOperations.GetValueFromDictionary(floatListData, name, new List<float>());
		}
		public List<double> GetDoubleListData(string name)
		{
			return StandardDataOperations.GetValueFromDictionary(doubleListData, name, new List<double>());
		}

		public List<List<int>> GetInteger2DListData(string name)
		{
			return StandardDataOperations.GetValueFromDictionary(integer2DListData, name, new List<List<int>>());
		}

		public Dictionary<string, double>.KeyCollection GetDoubleDataKeys()
		{
			return doubleData.Keys;
		}
		public Dictionary<string, decimal>.KeyCollection GetDecimalDataKeys()
		{
			return decimalData.Keys;
		}

		public void SetStringData(string name, string value)
		{
			stringData[name] = value;
		}

		public override string ToString()
		{
			return ToString(stringData) + ToString(integerData) + ToString(doubleData) + ToString(boolData);
		}
	}
}
