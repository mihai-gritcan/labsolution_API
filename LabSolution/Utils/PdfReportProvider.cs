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
    public interface IPdfReportProvider
    {
        Task<byte[]> CreatePdfReport(string fileName, ProcessedOrderForPdf processedOrderForPdf, LabConfigAddresses configOptions);
    }

    public class PdfReportProvider: IPdfReportProvider
    {
        private readonly IConverter _converter;

        public PdfReportProvider(IConverter converter)
        {
            _converter = converter;
        }

        public async Task<byte[]> CreatePdfReport(string fileName, ProcessedOrderForPdf processedOrderForPdf, LabConfigAddresses configOptions)
        {
            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(processedOrderForPdf.NumericCode);

            var path = $"{configOptions.DownloadPDFUrl}{fileName}";

            var qrCode = QRCodeProvider.GeneratQRCode(path);

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

            var htmlContent = await TemplateBuilder.GetReportTemplate(processedOrderForPdf, barcode, qrCode, configOptions);

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

    public static class PdfTemplateLoader
    {
        public static async Task<string> GetDefaultTemplateHtml(TestType testType)
        {
            var templateName = string.Empty;
            switch (testType)
            {
                case TestType.Antigen:
                    templateName = "testAntigenRo_En_Ru";
                    break;
                case TestType.PCR:
                case TestType.PCRExpress:
                    templateName = "testPCRRo_En_Ru";
                    break;
                case TestType.AntibodyNeutralizing:
                    templateName = "testAntibodyNeutralizingRo_En_Ru";
                    break;
                case TestType.Antibody:
                    templateName = "testAntibodyRo_En_Ru";
                    break;
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "Templates", $"{templateName}.html");
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }
    }
}
