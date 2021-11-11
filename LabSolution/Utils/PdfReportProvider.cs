using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace LabSolution.Utils
{
    public interface IPdfReportProvider
    {
        Task<byte[]> CreatePdfReport(ProcessedOrderForPdf processedOrderForPdf);
    }

    public class PdfReportProvider: IPdfReportProvider
    {
        private readonly IConverter _converter;
        private readonly LabConfigOptions _labConfigOptions;

        public PdfReportProvider(IConverter converter, IOptions<LabConfigOptions> labConfigOptions)
        {
            _converter = converter;
            _labConfigOptions = labConfigOptions.Value;
        }

        public async Task<byte[]> CreatePdfReport(ProcessedOrderForPdf processedOrderForPdf)
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
                // Out = fullyQualifiedFilePath, // when 'Out' is set Convert doesn't return the PDF file as bytes
                DPI = 400
            };

            var htmlContent = await TemplateBuilder.GetReportTemplate(processedOrderForPdf, barcode, qrCode, _labConfigOptions);

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "Templates", "styles.css") },
                //HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "[page]/[toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = false, Right = "[page]/[toPage]" }
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
        private const string _orderProcessedByKey = "#ORDER_PROCESSED_BY_KEY";


        private const string _sampleIdKey = "#SAMPLE_ID_KEY";
        private const string _testResultKey = "#TEST_RESULT_KEY";
        private const string _testResultCommentKey = "#TEST_RESULT_COMMENT_KEY";
        private const string _testEquipmentAnalyzerKey = "#TEST_EQUIPMENT_ANALYZER_KEY";

        private static string IsVirusConfirmed(TestResult testResult, TestLanguage testLanguage)
        {
            return testLanguage switch
            {
                TestLanguage.Romanian => testResult == TestResult.Positive ? "prezența" : "absența",
                TestLanguage.English => testResult == TestResult.Positive ? "presence" : "absence",
                _ => testResult == TestResult.Positive ? "presence" : "absence",
            };
        }

        public static async Task<string> GetReportTemplate(ProcessedOrderForPdf processedOrderForPdf, byte[] barcode, byte[] qrcode, LabConfigOptions labConfigOptions)
        {
            var htmlTemplate = await TemplateLoader.GetDefaultTemplateHtml(processedOrderForPdf.TestLanguage, processedOrderForPdf.TestType);
            
            return htmlTemplate
                .Replace(_labNameKey, labConfigOptions.Name)
                .Replace(_labAddressKey, labConfigOptions.Address)
                .Replace(_labPhoneKey, labConfigOptions.Phone)
                .Replace(_labSiteKey, labConfigOptions.Site)

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

                .Replace(_orderProcessingDateTimeKey, processedOrderForPdf.ProcessedAt.ToString("dd/MM/yyyy HH:mm"))
                .Replace(_orderReceivedInLabDateTimeKey, processedOrderForPdf.OrderDate.ToString("dd/MM/yyyy HH:mm"))
                .Replace(_dateOfExaminationKey, processedOrderForPdf.OrderDate.ToString("dd/MM/yyyy"))
                .Replace(_orderProcessedByKey, processedOrderForPdf.ProcessedBy)

                .Replace(_sampleIdKey, processedOrderForPdf.OrderId.ToString())
                .Replace(_testResultKey, processedOrderForPdf.TestResult.ToString())
                .Replace(_testResultCommentKey, IsVirusConfirmed(processedOrderForPdf.TestResult, processedOrderForPdf.TestLanguage))
                .Replace(_testEquipmentAnalyzerKey, labConfigOptions.TestEquipmentAnalyzer);
        }

        private static int CalculateCustomerAge(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow.ToBucharestTimeZone().Date;
            var age = today.Year - dateOfBirth.Year;
            // Go back to the year in which the person was born in case of a leap year
            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }
    }

    public static class TemplateLoader
    {
        public static async Task<string> GetDefaultTemplateHtml(TestLanguage testLanguage, TestType testType)
        {
            var templateName = string.Empty;
            switch (testType)
            {
                case TestType.Antigen:
                    templateName = $"testAntigen".AppendLanguageSuffix(testLanguage);
                    break;
                case TestType.PCR:
                    templateName = "testPcr".AppendLanguageSuffix(testLanguage);
                    break;
                case TestType.Antibody:
                    templateName = "testAntibodyRo_En_Ru";
                    break;
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "Templates", $"{templateName}.html");
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }

        private static string AppendLanguageSuffix(this string fileName, TestLanguage testLanguage)
        {
            return testLanguage == TestLanguage.Romanian ? $"{fileName}Ro" : $"{fileName}En";
        }
    }
}
