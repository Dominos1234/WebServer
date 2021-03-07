
namespace Core.Text
{
	class TextContants
	{
		private static char[] alphanumericCharacters = null;

		public static char[] AlphanumericCharacters
		{
			get
			{
				if (alphanumericCharacters == null)
				{
					var alphanumericUpperCaseCharactersLength = 'Z' - 'A';
					var alphanumericLowerCaseCharactersLength = 'z' - 'a';
					var alphanumericNumericCharactersLength = '9' - '0';

					var alphanumericCharactersLength = alphanumericUpperCaseCharactersLength + alphanumericLowerCaseCharactersLength + alphanumericNumericCharactersLength;
					var offset = 0;

					alphanumericCharacters = new char[alphanumericCharactersLength];

					for (var index = 0; index < alphanumericUpperCaseCharactersLength; ++index, ++offset)
					{
						alphanumericCharacters[offset] = (char)('A' + index);
					}

					for (var index = 0; index < alphanumericLowerCaseCharactersLength; ++index, ++offset)
					{
						alphanumericCharacters[offset] = (char)('a' + index);
					}

					for (var index = 0; index < alphanumericNumericCharactersLength; ++index, ++offset)
					{
						alphanumericCharacters[offset] = (char)('0' + index);
					}
				}

				return alphanumericCharacters;
			}
		}
	}
}
