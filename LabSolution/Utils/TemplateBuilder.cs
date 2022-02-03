using LabSolution.Dtos;
using LabSolution.Enums;
using LabSolution.HttpModels;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace LabSolution.Utils
{

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

        private const string _testResultKeyRo = "#TEST_RESULT_KEY_RO";
        private const string _testResultKeyEn = "#TEST_RESULT_KEY_EN";
        private const string _testResultKeyRu = "#TEST_RESULT_KEY_RU";

        private const string _testResultAntibodyQtyUnitsKey = "#TEST_RESULT_ANTIBODY_QTY_UNITS_KEY";

        private const string _testResultCommentKeyRo = "#TEST_RESULT_COMMENT_KEY_RO";
        private const string _testResultCommentKeyEn = "#TEST_RESULT_COMMENT_KEY_EN";
        private const string _testResultCommentKeyRu = "#TEST_RESULT_COMMENT_KEY_RU";

        // TODO: #56 Use StringBuilder instead of string.Replace
        public static async Task<string> GetReportTemplate(ProcessedOrderForPdf processedOrderForPdf, byte[] barcode, byte[] qrcode, LabConfigAddresses labConfigOptions)
        {
            var htmlTemplate = await PdfTemplateLoader.GetDefaultTemplateHtml(processedOrderForPdf.TestType);

            var result = htmlTemplate
                .Replace(_labNameKey, labConfigOptions.LabName)
                .Replace(_labAddressKey, labConfigOptions.LabAddress)
                .Replace(_labPhoneKey, labConfigOptions.PhoneNumber)
                .Replace(_labSiteKey, labConfigOptions.WebSiteAddress)

                .Replace(_barcodeKey, Convert.ToBase64String(barcode))
                .Replace(_qrcodeKey, Convert.ToBase64String(qrcode))

                .Replace(_customerFirstNameKey, processedOrderForPdf.Customer.FirstName)
                .Replace(_customerLastNameKey, processedOrderForPdf.Customer.LastName)
                .Replace(_customerDOBKey, processedOrderForPdf.Customer.DateOfBirth.ToString("dd/MM/yyyy"))
                .Replace(_customerAgeKey, CalculateCustomerAge(processedOrderForPdf.Customer.DateOfBirth).ToString())
                .Replace(_customerGenderKey, processedOrderForPdf.Customer.Gender == Gender.Male ? "M" : "F")
                .Replace(_customerPersonalNumberKey, processedOrderForPdf.Customer.PersonalNumber)
                .Replace(_customerPassportNumberKey, processedOrderForPdf.Customer.Passport ?? string.Empty)
                .Replace(_customerPhoneKey, processedOrderForPdf.Customer.Phone ?? string.Empty)
                .Replace(_customerAddressKey, processedOrderForPdf.Customer.Address ?? string.Empty)
                .Replace(_customerEmailKey, processedOrderForPdf.Customer.Email ?? string.Empty)

                .Replace(_orderProcessingDateTimeKey, processedOrderForPdf.ProcessedAt.ToString("dd/MM/yyyy HH:mm"))
                .Replace(_orderReceivedInLabDateTimeKey, ComputeOrderReceivedInLabTime(processedOrderForPdf.ProcessedAt).ToString("dd/MM/yyyy HH:mm"))
                .Replace(_dateOfExaminationKey, processedOrderForPdf.OrderDate.ToString("dd/MM/yyyy"))
                .Replace(_orderProcessedByKey, processedOrderForPdf.ProcessedBy)

                .Replace(_sampleIdKey, processedOrderForPdf.OrderId.ToString());

            return AppendTestResult(result, processedOrderForPdf);
        }

        private static string AppendTestResult(string htmlTemplate, ProcessedOrderForPdf processedOrderForPdf)
        {
            return processedOrderForPdf.TestType == TestType.AntibodyNeutralizing || processedOrderForPdf.TestType == TestType.Antibody
                ? htmlTemplate.Replace(_testResultAntibodyQtyUnitsKey, processedOrderForPdf.ResultQtyUnits?.ToString())
                : htmlTemplate
                    .Replace(_testResultKeyRo, GetTestResultText(processedOrderForPdf.TestResult, TestLanguage.Romanian))
                    .Replace(_testResultKeyEn, GetTestResultText(processedOrderForPdf.TestResult, TestLanguage.English))
                    .Replace(_testResultKeyRu, GetTestResultText(processedOrderForPdf.TestResult, TestLanguage.Russian))
                    .Replace(_testResultCommentKeyRo, IsVirusConfirmed(processedOrderForPdf.TestResult, TestLanguage.Romanian))
                    .Replace(_testResultCommentKeyEn, IsVirusConfirmed(processedOrderForPdf.TestResult, TestLanguage.English))
                    .Replace(_testResultCommentKeyRu, IsVirusConfirmed(processedOrderForPdf.TestResult, TestLanguage.Russian));
        }

        private static int CalculateCustomerAge(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow.ToBucharestTimeZone().Date;
            var age = today.Year - dateOfBirth.Year;
            // Go back to the year in which the person was born in case of a leap year
            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }

        /// <summary> By requirements and order should be 15 minutes later than the DateTime when the sample was taken </summary>
        private static DateTime ComputeOrderReceivedInLabTime(DateTime dateTime) => dateTime.AddMinutes(15);

        private static string GetTestResultText(TestResult testResult, TestLanguage testLanguage)
        {
            return testLanguage switch
            {
                TestLanguage.Romanian => testResult == TestResult.Positive ? "Pozitiv" : "Negativ",
                TestLanguage.English => testResult == TestResult.Positive ? "Positive" : "Negative",
                TestLanguage.Russian => testResult == TestResult.Positive ? "Положительный" : "Отрицательный",
                _ => testResult == TestResult.Positive ? "Positive" : "Negative",
            };
        }

        private static string IsVirusConfirmed(TestResult testResult, TestLanguage testLanguage)
        {
            return testLanguage switch
            {
                TestLanguage.Romanian => testResult == TestResult.Positive ? "prezența" : "absența",
                TestLanguage.English => testResult == TestResult.Positive ? "presence" : "absence",
                TestLanguage.Russian => testResult == TestResult.Positive ? "присутствие" : "отсутствие",
                _ => testResult == TestResult.Positive ? "presence" : "absence",
            };
        }
    }
}
