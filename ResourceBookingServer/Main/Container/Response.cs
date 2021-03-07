namespace ResourceBookingServer.Container
{
	class Response
	{
		public static readonly Response missingFields = new Response("missingFields");
		public static readonly Response invalidData = new Response("invalidData");

		public static readonly Response entityAlreadyExists = new Response("entityAlreadyExists");
		public static readonly Response entityNotExists = new Response("entityNotExists");

		public static readonly Response incorrectLogin = new Response("incorrectLogin");
		public static readonly Response notLogged = new Response("notLogged");
		public static readonly Response missingPermissions = new Response("missingPermissions");
		public static readonly Response masterAdminNotAllowed = new Response("masterAdminNotAllowed");

		public static readonly Response bookingExistingInSameInterval = new Response("bookingExistingInSameInterval");

		public static readonly Response correct = new Response("correct", true);

		public string Code { get; }
		public object Data { get; }

		public Response(string code)
		{
			Code = code;
		}
		public Response(string code, object data)
		{
			Code = code;
			Data = data;
		}

		public Response WithData(object data)
		{
			return new Response(Code, data);
		}
	}
}
