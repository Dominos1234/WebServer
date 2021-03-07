using System;
using System.Collections.Generic;
using System.Globalization;

namespace Core.Text
{
	class TextOperations
	{
		private static readonly int unicodeSequenceLength = 5;

		public static List<string> GetTextBetweenTags(string text, string preTag, string postTag)
		{
			List<string> result = new List<string>();

			string[] preSeparated = text.Split(new string[] { preTag }, StringSplitOptions.None);

			for (int index = 1; index < preSeparated.Length; ++index)
			{
				string[] postSeparated = preSeparated[index].Split(new string[] { postTag }, StringSplitOptions.None);

				if (postSeparated.Length > 1)
				{
					result.Add(postSeparated[0]);
				}
			}

			return result;
		}

		public static dynamic GetCastedText(string value, string type, out Type valueType)
		{
			dynamic returnValue;

			if (type == "Int")
			{
				returnValue = int.Parse(value);
			}
			else if (type == "Float")
			{
				returnValue = float.Parse(value, CultureInfo.InvariantCulture);
			}
			else if (type == "Double")
			{
				returnValue = double.Parse(value, CultureInfo.InvariantCulture);
			}
			else if (type == "Bool")
			{
				returnValue = bool.Parse(value);
			}
			else if (type == "Decimal")
			{
				returnValue = decimal.Parse(value);
			}
			else
			{
				returnValue = value;
			}

			valueType = returnValue.GetType();

			return returnValue;
		}
		public static List<dynamic> GetCastedText(dynamic values, string type, out Type valueType)
		{
			var castedValues = new List<dynamic>();

			valueType = null;
			
			foreach (var value in values)
			{
				castedValues.Add(GetCastedText(value, type, out valueType));
			}

			return castedValues;
		}

		public static List<string> GetTextInBrackets(string text)
		{
			var values = new List<string>();

			var bracketIndex = 0;
			var textInBrackets = "";

			foreach (var character in text)
			{
				var addCharacter = false;

				if (character == '(')
				{
					bracketIndex += 1;

					if (bracketIndex > 1)
					{
						addCharacter = true;
					}
				}
				else if (character == ')')
				{
					bracketIndex -= 1;

					if (bracketIndex == 0)
					{
						values.Add(textInBrackets);
						textInBrackets = "";
					}
					else
					{
						addCharacter = true;
					}
				}
				else
				{
					addCharacter = bracketIndex > 0;
				}

				if (addCharacter)
				{
					textInBrackets += character;
				}
			}

			return values;
		}
		public static dynamic GetSeparatedTextInBrackets(string text, string separator, int dimensionsAmount)
		{
			dynamic result;

			if (dimensionsAmount == -1)
			{
				result = text;
			}
			else if (dimensionsAmount == 0)
			{
				if (separator == null)
				{
					result = text;
				}
				else
				{
					result = new List<string>(text.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
				}
			}
			else
			{
				var valueData = text.Substring(1, text.Length - 2);
				var values = GetTextInBrackets(valueData);

				result = new List<dynamic>();

				foreach (var value in values)
				{
					result.Add(GetSeparatedTextInBrackets(value, separator, dimensionsAmount - 1));
				}
			}

			return result;
		}
		public static dynamic GetSeparatedTextInBrackets(string text, string separator, out int dimensionsAmount)
		{
			dimensionsAmount = 0;

			for (var index = 0; index < text.Length / 2; ++index)
			{
				if (text[index] != '(' || text[text.Length - index - 1] != ')')
				{
					dimensionsAmount = index;
					break;
				}
			}

			if (dimensionsAmount == 1)
			{
				text = text.Substring(1, text.Length - 2);
			}

			return GetSeparatedTextInBrackets(text, separator, dimensionsAmount - 1);
		}

		public static List<string> GetUnicodeTextLetters(string text)
		{
			var result = new List<string>();

			for (var index = 0; index < text.Length; ++index)
			{
				if (text[index] == '\\')
				{
					if (index + unicodeSequenceLength < text.Length)
					{
						index += 1;

						string unicodeCharacter = "";

						for (var secondaryIndex = 0; secondaryIndex < unicodeSequenceLength; ++secondaryIndex)
						{
							unicodeCharacter += text[index + secondaryIndex];
						}

						result.Add(unicodeCharacter);
						index += unicodeSequenceLength - 1;
					}
					else
					{
						break;
					}
				}
				else
				{
					result.Add(text[index].ToString());
				}
			}

			return result;
		}

		public static string GetNumberWithPadding(int number, int numberSize)
		{
			var textNumber = number.ToString();
			var padding = "";

			for (var index = textNumber.Length; index < numberSize; ++index)
			{
				padding += "0";
			}

			return padding + textNumber;
		}

		public static bool IsArrayContainsText(string[] array, string text)
		{
			foreach (var value in array)
			{
				if (value == text)
				{
					return true;
				}
			}

			return false;
		}
	}
}
