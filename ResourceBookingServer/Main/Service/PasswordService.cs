using Core.Operations;
using System;
using System.Text;

namespace ResourceBookingServer.Service
{
	class PasswordService
	{
		private static readonly int passwordHashIterations = 10000;
		private static readonly int passwordHashSize = 64;

		private readonly byte[] passwordHashSalt;

		public PasswordService(string passwordHashSalt)
		{
			this.passwordHashSalt = Encoding.ASCII.GetBytes(passwordHashSalt);
		}

		public string GetHash(string password)
		{
			var bytes = CryptographyOperations.GetHashedPassword(password, passwordHashSalt, passwordHashIterations, passwordHashSize);

			return Convert.ToBase64String(bytes);
		}
	}
}
