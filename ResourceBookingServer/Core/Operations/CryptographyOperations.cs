using Core.Text;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Core.Operations
{
	class CryptographyOperations
	{
		private static readonly char[] specialCharacters = new char[] { '!', '?', '@' };
		private static readonly HashSet<char> base64CharactersToRemove = new HashSet<char> { '+', '/', '=' };

		public static byte[] GetRandomBytes(int length)
		{
			using (var randomNumberGenerator = new RNGCryptoServiceProvider())
			{
				var randomData = new byte[length];

				randomNumberGenerator.GetBytes(randomData);

				return randomData;
			}
		}

		public static int GetRandomNumber(int from, int to)
		{
			if (to <= from)
			{
				return 0;
			}
			else
			{
				var bytes = GetRandomBytes(4);
				var number = BitConverter.ToUInt32(bytes, 0);

				return (int)(number % ((to - from + 1))) + from;
			}
		}

		public static string GetRandomString(int length, int specialCharactersMinAmount = 0, int specialCharactersMaxAmount = 0)
		{
			var inputLength = MathOperations.GetBase64InputLength(length);
			var randomStringBytes = Encoding.ASCII.GetBytes(Convert.ToBase64String(GetRandomBytes(inputLength)));

			for (var index = 0; index < randomStringBytes.Length; ++index)
			{
				if (base64CharactersToRemove.Contains((char)randomStringBytes[index]))
				{
					var characterIndex = GetRandomNumber(0, TextContants.AlphanumericCharacters.Length - 1);

					randomStringBytes[index] = (byte)TextContants.AlphanumericCharacters[characterIndex];
				}
			}

			if (specialCharactersMinAmount < specialCharactersMaxAmount)
			{
				var specialCharactersAmount = GetRandomNumber(specialCharactersMinAmount, specialCharactersMaxAmount);

				for (var index = 0; index < specialCharactersAmount; ++index)
				{
					var specialCharacterPosition = GetRandomNumber(0, randomStringBytes.Length - 1);
					var specialCharacter = specialCharacters[GetRandomNumber(0, specialCharacters.Length - 1)];

					randomStringBytes[specialCharacterPosition] = (byte)specialCharacter;
				}
			}

			return Encoding.ASCII.GetString(randomStringBytes);
		}

		public static byte[] GetHashedPassword(string password, byte[] salt, int iterations, int hashSize)
		{
			return new Rfc2898DeriveBytes(password, salt, iterations).GetBytes(hashSize);
		}
	}
}
