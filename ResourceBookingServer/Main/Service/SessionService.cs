using Core.Operations;
using System.Collections.Generic;

namespace ResourceBookingServer.Service
{
	class SessionService
	{
		private static readonly int sessionIdSize = 128;

		private readonly object sessionsLockObject = new object();

		private readonly Dictionary<string, string> sessions;

		private string GetNewSessionId()
		{
			while (true)
			{
				var id = CryptographyOperations.GetRandomString(sessionIdSize);

				if (!sessions.ContainsKey(id))
				{
					return id;
				}
			}
		}

		public SessionService()
		{
			sessions = new Dictionary<string, string>();
		}

		public string CreateUserSession(string login)
		{
			lock (sessionsLockObject)
			{
				string sessionId = null;

				foreach (var session in sessions)
				{
					if (session.Value == login)
					{
						sessionId = session.Key;
						break;
					}
				}

				if (sessionId != null)
				{
					sessions.Remove(sessionId);
				}

				var id = GetNewSessionId();

				sessions[id] = login;

				return id;
			}
		}

		public void DestroyUserSession(string sessionId)
		{
			lock (sessionsLockObject)
			{
				sessions.Remove(sessionId);
			}
		}

		public string GetLoggedUser(string sessionId)
		{
			if (sessionId == null)
			{
				return null;
			}

			lock (sessionsLockObject)
			{
				if (sessions.ContainsKey(sessionId))
				{
					return sessions[sessionId];
				}
				else
				{
					return null;
				}
			}
		}
	}
}
