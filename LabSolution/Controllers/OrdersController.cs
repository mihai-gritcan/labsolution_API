using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace LabSolution.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IPdfReportProvider _pdfReportProvider;

        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ICustomerService customerService, IOrderService orderService, IPdfReportProvider pdfReportProvider, ILogger<OrdersController> logger)
        {
            _customerService = customerService;
            _orderService = orderService;
            _pdfReportProvider = pdfReportProvider;
            _logger = logger;
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

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var allSavedCustomers = await _customerService.SaveCustomers(createOrder.Customers);

                var addedOrders = await _orderService.SaveOrders(createOrder, allSavedCustomers);

                savedOrders.AddRange(addedOrders.Select(x => new CreatedOrdersResponse
                {
                    OrderId = x.Id,
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
                OrderId = savedProcessedOrder.Id,
                NumericCode = numericCode7Digits,
                Barcode = Convert.ToBase64String(barcode),
                ProcessedAt = savedProcessedOrder.ProcessedAt,
                TestLanguage = (TestLanguage)orderDetails.TestLanguage,
                TestType = (TestType)orderDetails.TestType,
                Customer = CustomerDto.CreateDtoFromEntity(orderDetails.Customer, orderDetails.ParentId == null)
            });
        }

        [HttpGet("{date}/processed")]
        public async Task<IActionResult> GetOrdersToSetResult(DateTime date, [FromQuery] string numericCode)
        {
            return Ok(await _orderService.GetOrdersToSetResult(date, numericCode));
        }

        // reception set test result by processedOrderId
        [HttpPatch("{processedOrderId}/settestresult")]
        public async Task<IActionResult> SetTestResult(int processedOrderId, SetTestResultRequest setResultRequest)
        {
            if (processedOrderId != setResultRequest.ProcessedOrderId)
                return BadRequest();

            await _orderService.SetTestResult(setResultRequest.ProcessedOrderId, setResultRequest.TestResult, setResultRequest.ExecutorName, setResultRequest.VerifierName, setResultRequest.ValidatorName);

            return NoContent();
        }

        [AllowAnonymous]
        // reception getPdfResult by processedOrderId
        [HttpGet("{processedOrderId}/pdfresult")]
        public async Task<IActionResult> GetPdfResult(int processedOrderId)
        {
            var processedOrderForPdf = await _orderService.GetProcessedOrderForPdf(processedOrderId);

            var reportsResultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "assets", "GeneratedReports");

            if (!string.IsNullOrEmpty(processedOrderForPdf.PdfName))
            {
                string path = Path.Combine(reportsResultDirectory, $"{processedOrderForPdf.PdfName}.pdf");

                if (System.IO.File.Exists(path))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    MemoryStream memoryStream = new MemoryStream(bytes);

                    return new FileStreamResult(memoryStream, "application/pdf");
                }
            }

            var pdfBytes = await _pdfReportProvider.CreatePdfReport(processedOrderForPdf);

            var fileName = $"{Guid.NewGuid()}";
            var fullyQualifiedFilePath = Path.Combine(reportsResultDirectory, $"{fileName}.pdf");

            using (var fs = new FileStream(fullyQualifiedFilePath, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(pdfBytes, 0, pdfBytes.Length);
            }

            await _orderService.SetPdfName(processedOrderId, fileName);

            MemoryStream ms = new MemoryStream(pdfBytes);
            return new FileStreamResult(ms, "application/pdf");
        }

        [AllowAnonymous]
        [HttpGet("{processedOrderId}/generate-streampdf")]
        public async Task<IActionResult> GeneratePdfResultOnTheFlyAsStream(int processedOrderId)
        {
            var processedOrderForPdf = await _orderService.GetProcessedOrderForPdf(processedOrderId);

            _logger.LogInformation("Start generating PDF as STREAM");

            try
            {
                var pdfBytes = await _pdfReportProvider.CreatePdfReport(processedOrderForPdf);
                _logger.LogInformation($"Finished generating PDF as STREAM. Length is {pdfBytes.Length}");

                MemoryStream ms = new MemoryStream(pdfBytes);
                return new FileStreamResult(ms, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error generating PDF as STREAM. {ex.Message}");
                return UnprocessableEntity();
            }
        }

        [AllowAnonymous]
        [HttpGet("{processedOrderId}/generate-filepdf")]
        public async Task<IActionResult> GeneratePdfResultOnTheFlyAsFile(int processedOrderId)
        {
            var processedOrderForPdf = await _orderService.GetProcessedOrderForPdf(processedOrderId);

            _logger.LogInformation("Start generating PDF as FILE");

            try
            {
                var pdfBytes = await _pdfReportProvider.CreatePdfReport(processedOrderForPdf);
                _logger.LogInformation($"Finished generating PDF as FILE. Length is {pdfBytes.Length}");


                _logger.LogInformation("Start writing bytes to Storage");
                var reportsResultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "assets", "GeneratedReports");
                var fileName = $"{Guid.NewGuid()}";
                var fullyQualifiedFilePath = Path.Combine(reportsResultDirectory, $"{fileName}.pdf");

                using (var fs = new FileStream(fullyQualifiedFilePath, FileMode.Create, FileAccess.Write))
                {
                    await fs.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
                _logger.LogInformation("End writing bytes to Storage");

                var stream = new FileStream(fullyQualifiedFilePath, FileMode.Open);
                return File(stream, "application/pdf", $"{fileName}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error generating PDF as FILE. {ex.Message}");
                return UnprocessableEntity();
            }
        }

        [AllowAnonymous]
        // reception getPdfResult by processedOrderId
        [HttpGet("{processedOrderId}/pdfresult/file")]
        public async Task<IActionResult> GetFile(int processedOrderId)
        {
            var reportsResultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "assets", "GeneratedReports");

            var stream = new FileStream(Path.Combine(reportsResultDirectory, $"demoPcrEn.pdf"), FileMode.Open);
            return File(stream, "application/pdf", $"demoPcrEn.pdf");
        }
    }
}
