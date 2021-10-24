using BarcodeLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LabSolution.Utils
{
    public static class BarcodeProvider
    {
        public static byte[] GenerateBarcodeFromNumericCode(string numericCode)
        {
            var barcode = new Barcode();
            var img = barcode.Encode(TYPE.CODE39, numericCode, Color.Black, Color.White, 250, 100);
            return ConvertImageToBytes(img);
        }

        public static byte[] GenerateBarcode(DateTime date, int customerOrderNumber)
        {
            var numericCode = GenerateNumericCode(date, customerOrderNumber);

            var barcode = new Barcode();
            var img = barcode.Encode(TYPE.CODE39, numericCode.ToString(), Color.Black, Color.White, 250, 100);
            return ConvertImageToBytes(img);
        }

        [Obsolete("This method is not allowed to be used because of verbosity")]
        // 2021-10-12 1:30 -> 110121130 + 5 digits from customerOrderNumber => 11012113000015
        private static long GenerateNumericCode(DateTime date, int customerOrderNumber)
        {
            var year4Digits = date.Year.ToString();
            var month2Digits = date.Month.ToString("D2");
            var day2Digits = date.Day.ToString("D2");
            var hour2Digits = date.Hour.ToString("D2");
            var minute2Digits = date.Minute.ToString("D2");

            var customerOrder5Digits = customerOrderNumber.ToString("D5");

            const int skipFirst3Digits = 3;
            var concatenated = $"{year4Digits}{month2Digits}{day2Digits}{hour2Digits}{minute2Digits}{customerOrder5Digits}";

            if (!long.TryParse(concatenated.Substring(skipFirst3Digits), out var numericCode))
                throw new ArgumentException($"Can't generat numeric code based on the input parameters: '{date}', '{customerOrderNumber}'");

            return numericCode;
        }

        private static byte[] ConvertImageToBytes(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
