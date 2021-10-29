using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using System;
using System.IO;
using System.Text;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace LabSolution.Utils
{
    public interface IPdfReportProvider
    {
        byte[] CreatePdfReport(string fullyQualifiedFilePath, ProcessedOrderForPdf processedOrderForPdf);
    }

    public class PdfReportProvider: IPdfReportProvider
    {
        private readonly IConverter _converter;

        public PdfReportProvider(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] CreatePdfReport(string fullyQualifiedFilePath, ProcessedOrderForPdf processedOrderForPdf)
        {
            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(processedOrderForPdf.NumericCode);
            var qrCode = QRCodeProvider.GeneratQRCode(processedOrderForPdf.NumericCode);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
                Out = fullyQualifiedFilePath,
                DPI = 400
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = TemplateBuilder.GetReportTemplate(processedOrderForPdf, barcode, qrCode),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css") },
                //HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "[page]/[toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Right = "[page]/[toPage]" }
            };
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }
    }

    public static class TemplateBuilder
    {
        private const string LAB_NAME = "UNIVERSUL DIAGNOSTIC";
        private const string LAB_ADDRESS = "mun. Chișinău, str. Gh. Asachi 54";
        private const string LAB_PHONE = "022-123-456";
        private const string LAB_SITE = "www.universdiagnostic.md";

        private const string _labNameKey = "#LAB_NAME_KEY";
        private const string _labAddressKey = "#LAB_ADDRESS_KEY";
        private const string _labPhoneKey = "#LAB_PHONE_KEY";
        private const string _labSiteKey = "#LAB_SITE_KEY";


        private const string _barcodeKey = "#BARCODE_KEY";
        private const string _qrcodeKey = "#QRCODE_KEY";


        private const string _customerFirstNameKey = "#CUSTOMER_FIRST_NAME_KEY";
        private const string _customerLastNameKey = "#CUSTOMER_LAST_NAME_KEY";
        private const string _customerDOBKey = "#CUSTOMER_DOB_KEY";
        private const string _customerAgeKey = "#CUSTOMER_AGE_KEY";
        private const string _customerGenderKey = "#CUSTOMER_GENDER_KEY";
        private const string _customerPersonalNumberKey = "#CUSTOMER_PERSONAL_NUMBER_KEY";
        private const string _customerPassportNumberKey = "#CUSTOMER_PASSPORT_NUMBER_KEY";
        private const string _customerPhoneKey = "#CUSTOMER_PHONE_KEY";
        private const string _customerAddressKey = "#CUSTOMER_ADDRESS_KEY";
        private const string _customerEmailKey = "#CUSTOMER_EMAIL_KEY";

        private const string _orderProcessingDateTimeKey = "#ORDER_PROCESSING_DATE_TIME";
        private const string _orderReceivedInLabDateTimeKey = "#ORDER_RECEIVED_IN_LAB_DATE_TIME";
        private const string _dateOfExaminationKey = "#DATE_OF_EXAMINATION_KEY";

        private const string _sampleIdKey = "#SAMPLE_ID_KEY";
        private const string _testResultKey = "#TEST_RESULT_KEY";

        public static string GetReportTemplate(ProcessedOrderForPdf processedOrderForPdf, byte[] barcode, byte[] qrcode)
        {
           
            var htmlTemplate = TemplateLoader.GetDefaultTemplateHtml(processedOrderForPdf.TestLanguage, processedOrderForPdf.TestType);
            var refinedTemplate = htmlTemplate
                .Replace(_labNameKey, LAB_NAME)
                .Replace(_labAddressKey, LAB_ADDRESS)
                .Replace(_labPhoneKey, LAB_PHONE)
                .Replace(_labSiteKey, LAB_SITE)

                .Replace(_barcodeKey, Convert.ToBase64String(barcode))
                .Replace(_qrcodeKey, Convert.ToBase64String(qrcode))

                .Replace(_customerFirstNameKey, processedOrderForPdf.Customer.FirstName)
                .Replace(_customerLastNameKey, processedOrderForPdf.Customer.LastName)
                .Replace(_customerDOBKey, processedOrderForPdf.Customer.DateOfBirth.ToString("dd/MM/yyyy"))
                .Replace(_customerAgeKey, CalculateCustomerAge(processedOrderForPdf.Customer.DateOfBirth).ToString())
                .Replace(_customerGenderKey, processedOrderForPdf.Customer.Gender == Gender.Male ? "M" : "F")
                .Replace(_customerPersonalNumberKey, processedOrderForPdf.Customer.PersonalNumber.ToString())
                .Replace(_customerPassportNumberKey, processedOrderForPdf.Customer.Passport ?? string.Empty)
                .Replace(_customerPhoneKey, processedOrderForPdf.Customer.Phone ?? string.Empty)
                .Replace(_customerAddressKey, processedOrderForPdf.Customer.Address ?? string.Empty)
                .Replace(_customerEmailKey, processedOrderForPdf.Customer.Email ?? string.Empty)

                .Replace(_orderProcessingDateTimeKey, processedOrderForPdf.OrderDate.ToString("dd/MM/yyyy HH:mm"))
                .Replace(_orderReceivedInLabDateTimeKey, processedOrderForPdf.OrderDate.ToString("dd/MM/yyyy HH:mm"))
                .Replace(_dateOfExaminationKey, processedOrderForPdf.OrderDate.ToString("dd/MM/yyyy"))

                .Replace(_sampleIdKey, processedOrderForPdf.Id.ToString())
                .Replace(_testResultKey, processedOrderForPdf.TestResult.ToString());

            return refinedTemplate;
        }

        private static int CalculateCustomerAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            // Go back to the year in which the person was born in case of a leap year
            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }
    }

    public static class TemplateLoader
    {
        public static string GetDefaultTemplateHtml(TestLanguage testLanguage, TestType testType)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "testAntigenRo.html");
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            return streamReader.ReadToEnd();
        }
    }
}
