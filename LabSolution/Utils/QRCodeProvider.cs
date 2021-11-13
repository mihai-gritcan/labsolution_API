﻿using QRCoder;
using System.Drawing;
using System.IO;

namespace LabSolution.Utils
{
    public static class QRCodeProvider
    {
        public static byte[] GeneratQRCode(string pdfNameHex, string labWebsite)
        {
            var qrCodeGenerator = new QRCodeGenerator();
            var path = $"";

            var qrCodeData = qrCodeGenerator.CreateQrCode(path, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            Bitmap bitmap = qrCode.GetGraphic(15);
            return ConvertBitmapToBytes(bitmap);
        }

        private static byte[] ConvertBitmapToBytes(Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
