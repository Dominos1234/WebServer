using QRCoder;
using System;
using System.Drawing.Imaging;
using System.IO;

namespace ResourceBookingServer.Service
{
	class QRCodeService
	{
		private static readonly int pixelsPerModule = 20;

		public string GetBase64UrlQRCode(string url)
		{
			var qrCodeData = new QRCodeGenerator().CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
			var qrCode = new QRCode(qrCodeData);
			var bitmap = qrCode.GetGraphic(pixelsPerModule);

			var memoryStream = new MemoryStream();

			bitmap.Save(memoryStream, ImageFormat.Png);

			var imageBytes = memoryStream.ToArray();

			return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
		}
	}
}
