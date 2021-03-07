using Core.DataStorage;
using Core.Network.Http.Native;
using Core.Operations;
using ResourceBookingServer.Database;
using ResourceBookingServer.Managers;
using ResourceBookingServer.Service;
using System;
using System.Globalization;

namespace ResourceBookingServer
{
	class ResourceBookingServer
	{
		private static readonly int port = 2067;

		private static readonly string linuxMainPath = "/root/ResourceBookingServer/";
		private static readonly string windowsMainPath = "C:/ResourceBookingServer/";

		private static readonly string httpPath = "Http/";
		private static readonly string configPath = "Config.save";

		static void Main(string[] args)
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			var mainPath = OperatingSystemOperations.IsWindowsSystem() ? windowsMainPath : linuxMainPath;
			var config = new Config(mainPath + configPath);

			var dbConnectionString = config.GetStringData("ConnectionString");
			var dbName = config.GetStringData("DatabaseName");

			var db = new Db(dbConnectionString, dbName);
			var passwordService = new PasswordService(config.GetStringData("PasswordHashSalt"));
			var qrCodeService = new QRCodeService();

			var qrCodeUrl = config.GetStringData("QRCodeUrl");

			var masterAdminSessionService = new SessionService();
			var adminSessionService = new SessionService();
			var userSessionService = new SessionService();

			var masterAdminManager = new MasterAdminManager(db, masterAdminSessionService, passwordService, config.GetStringData("MasterKey"));
			var adminManager = new AdminManager(db, adminSessionService, passwordService, qrCodeService, qrCodeUrl, masterAdminSessionService);
			var userManager = new UserManager(db, userSessionService, passwordService);
			var publicManager = new PublicManager(db);

			new HttpServer(mainPath + httpPath, port, httpServerDataProviders: new Func<IDataProvider>[]
			{
				() => masterAdminManager,
				() => adminManager,
				() => userManager,
				() => publicManager
			});
		}
	}
}
