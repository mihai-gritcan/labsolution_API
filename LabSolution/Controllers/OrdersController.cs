using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace LabSolution.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IConverter _converter;

        public OrdersController(ICustomerService customerService, IOrderService orderService, IConverter converter)
        {
            _customerService = customerService;
            _orderService = orderService;
            _converter = converter;
        }

        // public order submit
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderRequest createOrder)
        {
            return Ok(await SaveOrder(createOrder));
        }

        // reception order submit
        [HttpPost("elevated")]
        public async Task<ActionResult> CreateElevatedOrder([FromBody] CreateOrderRequest createOrder)
        {
            return Ok(await SaveOrder(createOrder));
        }

        private async Task<IEnumerable<CreatedOrdersResponse>> SaveOrder(CreateOrderRequest createOrder)
        {
            var savedOrders = new List<CreatedOrdersResponse>();

            using(var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var allSavedCustomers = await _customerService.SaveCustomers(createOrder.Customers);

                var addedOrders = await _orderService.SaveOrders(createOrder, allSavedCustomers);

                savedOrders.AddRange(addedOrders.Select(x => new CreatedOrdersResponse
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Customer = CustomerDto.CreateDtoFromEntity(allSavedCustomers.Find(c => c.Id == x.CustomerId), isRootCustomer: x.ParentId == null),
                    ParentId = x.ParentId,
                    PlacedAt = x.PlacedAt,
                    Scheduled = x.Scheduled,
                    TestLanguage = (TestLanguage)x.TestLanguage,
                    TestType = (TestType)x.TestType
                }));

                scope.Complete();
            }

            return savedOrders;
        }

        // reception getOrders ByDate or idnp
        [HttpGet("{date}")]
        public async Task<ActionResult<object>> GetOrders(DateTime date, [FromQuery] long? idnp)
        {
            return Ok(await _orderService.GetOrdersWithStatus(date, idnp));
        }

        // reception updateOrder 
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, UpdateOrderRequest updateOrderRequest)
        {
            if (orderId != updateOrderRequest.Id)
                return BadRequest();

            try
            {
                await _orderService.UpdateOrder(updateOrderRequest);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // reception start printing the command confirmation
        [HttpPatch("{orderId}/processing-ticket")]
        public async Task<ActionResult<ProcessedOrderResponse>> GetProcessingTicket(int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null)
                return NotFound();

            var savedProcessedOrder = await _orderService.CreateOrUpdateProcessedOrder(orderId);
            var numericCode7Digits = savedProcessedOrder.Id.ToString("D7");
            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(numericCode7Digits);

            return Ok(new ProcessedOrderResponse
            {
                Id = savedProcessedOrder.Id,
                NumericCode = numericCode7Digits,
                Barcode = Convert.ToBase64String(barcode),
                ProcessedAt = savedProcessedOrder.ProcessedAt,
                TestLanguage = (TestLanguage)orderDetails.TestLanguage,
                TestType = (TestType)orderDetails.TestType,
                Customer = CustomerDto.CreateDtoFromEntity(orderDetails.Customer, orderDetails.ParentId == null)
            });
        }

        // reception set test result by processedOrderId
        [HttpPatch("{processedOrderId}/settestresult")]
        public async Task<IActionResult> SetTestResult(int processedOrderId, SetTestResultRequest setResultRequest)
        {
            if (processedOrderId != setResultRequest.ProcesedOrderId)
                return BadRequest();

            await _orderService.SetTestResult(setResultRequest.ProcesedOrderId, setResultRequest.TestResult, setResultRequest.ExecutorName, setResultRequest.VerifierName, setResultRequest.ValidatorName);

            return NoContent();
        }

        [AllowAnonymous]
        // reception getPdfResult by processedOrderId
        [HttpGet("{id}/pdfresult")]
        public async Task<IActionResult> GetPdfResult(int id)
        {
            var processedOrderForPdf = await _orderService.GetProcessedOrderForPdf(id);

            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(processedOrderForPdf.NumericCode);
            var qrCode = QRCodeProvider.GeneratQRCode(processedOrderForPdf.NumericCode);

            var fileName = $"antigenRo-{Guid.NewGuid()}";
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
                Out = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedReports", $"{fileName}.pdf"),
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
            _converter.Convert(pdf);
            return Ok("Successfully created PDF document.");
        }
    }

    public static class TemplateBuilder
    {
        private const string _barcodeKey = "#BARCODE_KEY";
        private const string _qrcodeKey = "#QRCODE_KEY";
        private const string _customerFirstNameKey = "#CUSTOMER_FIRST_NAME_KEY";
        private const string _customerLastNameKey = "#CUSTOMER_LAST_NAME_KEY";


        public static string GetReportTemplate(ProcessedOrderForPdf processedOrderForPdf, byte[] barcode, byte[] qrcode)
        {
            var htmlTemplate = TemplateLoader.GetDefaultTemplateHtml(processedOrderForPdf.TestLanguage, processedOrderForPdf.TestType);
            var refinedTemplate = htmlTemplate
                .Replace(_barcodeKey, Convert.ToBase64String(barcode))
                .Replace(_qrcodeKey, Convert.ToBase64String(qrcode))
                .Replace(_customerFirstNameKey, processedOrderForPdf.Customer.FirstName)
                .Replace(_customerLastNameKey, processedOrderForPdf.Customer.LastName);

            return refinedTemplate;
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
