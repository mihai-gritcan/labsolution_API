﻿using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Enums;
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
using LabSolution.EmailService;
using Microsoft.Extensions.Options;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IPdfReportProvider _pdfReportProvider;

        private readonly ILogger<OrdersController> _logger;

        private readonly IAppConfigService _appConfigService;

        private readonly IEmailSender _emailSender;

        private readonly AppEmailNotificationConfig _appEmailNotificationConfig;

        public OrdersController(ICustomerService customerService, IOrderService orderService,
            IPdfReportProvider pdfReportProvider, ILogger<OrdersController> logger,
            IAppConfigService appConfigService, IEmailSender emailSender,
            IOptions<AppEmailNotificationConfig> options)
        {
            _customerService = customerService;
            _orderService = orderService;
            _pdfReportProvider = pdfReportProvider;
            _logger = logger;
            _appConfigService = appConfigService;
            _emailSender = emailSender;

            _appEmailNotificationConfig = options.Value;
        }

        // public order submit
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderRequest createOrder)
        {
            var savedOrders = await SaveOrder(createOrder);

            if (_appEmailNotificationConfig.SendNotificationForOnlineBooking)
            {
                var labConfigs = await _appConfigService.GetLabConfigAddresses();
                await NotifyOrdersCreated(savedOrders, labConfigs);
            }

            return Ok(savedOrders);
        }

        // reception order submit
        [HttpPost("elevated")]
        public async Task<ActionResult> CreateElevatedOrder([FromBody] CreateOrderRequest createOrder)
        {
            var savedOrders = await SaveOrder(createOrder);
            if (_appEmailNotificationConfig.SendNotificationForInHouseBooking)
            {
                var labConfigs = await _appConfigService.GetLabConfigAddresses();
                await NotifyOrdersCreated(savedOrders, labConfigs);
            }

            return Ok(savedOrders);
        }

        // reception getOrders ByDate or idnp
        [HttpGet("{date}")]
        public async Task<ActionResult<object>> GetOrders(DateTime date, [FromQuery] string idnp)
        {
            return Ok(await _orderService.GetOrdersWithStatus(date, idnp));
        }

        // reception getOrders in range Date
        [HttpGet]
        public async Task<ActionResult<object>> GetOrders([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] TestType? type = null)
        {
            return Ok(await _orderService.GetOrdersWithStatus(start, end, type));
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var isSuperUserValue = User?.Claims.FirstOrDefault(x => x.Type.Equals(LabSolutionClaimsNames.UserIsSuperUser))?.Value;
            var canRemovePdfs = isSuperUserValue?.Equals("true", StringComparison.InvariantCultureIgnoreCase) == true;

            await _orderService.DeleteOrder(orderId, canRemovePdfs);

            return NoContent();
        }

        // reception updateOrder 
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, UpdateOrderRequest updateOrderRequest)
        {
            if (orderId != updateOrderRequest.Id)
                return BadRequest();

            await CheckIsLabOpen(updateOrderRequest.ScheduledDateTime);

            await _orderService.UpdateOrder(updateOrderRequest);

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
                ProcessedOrderId = savedProcessedOrder.Id,
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

            var labConfigs = await _appConfigService.GetLabConfigAddresses();
            var fileName = $"{Guid.NewGuid()}";

            ProcessedOrderForPdf processedOrderForPdf;
            byte[] pdfBytes;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _orderService.SetTestResult(setResultRequest.ProcessedOrderId, setResultRequest.TestResult,
                    setResultRequest.ExecutorName, setResultRequest.VerifierName, setResultRequest.ValidatorName);

                processedOrderForPdf = await _orderService.GetProcessedOrderForPdf(processedOrderId);

                pdfBytes = await _pdfReportProvider.CreatePdfReport(fileName, processedOrderForPdf, labConfigs);

                await _orderService.SaveOrReplacePdfBytes(processedOrderForPdf.OrderId, fileName, pdfBytes);

                scope.Complete();
            }

            if (pdfBytes is null)
                return BadRequest("Something went wrong during PDF creation. Please retry");

            if(_appEmailNotificationConfig.SendNotificationWhenTestIsCompleted && !string.IsNullOrWhiteSpace(processedOrderForPdf.Customer.Email))
                await NotifyOrderCompleted(processedOrderForPdf, labConfigs, pdfBytes);

            return NoContent();
        }

        // reception getPdfResult by processedOrderId
        [HttpGet("{processedOrderId}/pdfresult-db")]
        public async Task<IActionResult> GetPdfResultStoreInDb(int processedOrderId)
        {
            var existingPdf = await _orderService.GetPdfBytes(processedOrderId);

            MemoryStream stream = new MemoryStream(existingPdf.PdfBytes);
            return new FileStreamResult(stream, "application/pdf");
        }

        private async Task<IEnumerable<CreatedOrdersResponse>> SaveOrder(CreateOrderRequest createOrder)
        {
            await CheckIsLabOpen(createOrder.ScheduledDateTime);

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
                    PlacedAt = DateTime.SpecifyKind(x.PlacedAt, DateTimeKind.Local),
                    Scheduled = DateTime.SpecifyKind(x.Scheduled, DateTimeKind.Local),
                    TestLanguage = (TestLanguage)x.TestLanguage,
                    TestType = (TestType)x.TestType
                }));

                scope.Complete();
            }

            return savedOrders;
        }

        private async Task NotifyOrdersCreated(IEnumerable<CreatedOrdersResponse> createdOrders, LabConfigAddresses labConfigs)
        {
            const string subject = "COVID-19 [ON-LINE booking]";

            foreach (var item in createdOrders.Where(x => !string.IsNullOrWhiteSpace(x.Customer.Email)))
            {
                var fullName = $"{item.Customer.LastName} {item.Customer.FirstName}";
                var content = $"{fullName}, Personal number: {item.Customer.PersonalNumber}. Reservation time: {item.Scheduled:yyyy-MM-dd HH:mm}. Address: {labConfigs.LabAddress}";

                await _emailSender.SendEmailAsync(new Message(new List<(string Name, string Address)> { (fullName, item.Customer.Email) }, subject, content));
            }
        }

        private async Task NotifyOrderCompleted(ProcessedOrderForPdf orderForPdf, LabConfigAddresses labConfigs, byte[] pdfBytes)
        {
            const string subject = "COVID-19 [Test result]";

            var fullName = $"{orderForPdf.Customer.LastName} {orderForPdf.Customer.FirstName}";
            var messageText = $"{fullName}, Order date: {orderForPdf.OrderDate:yyyy-MM-dd HH:mm}. Test is completed. Result: {orderForPdf.TestResult}";
            var attachmentName = fullName.Replace(" ", "");
            attachmentName = $"{attachmentName}_{orderForPdf.TestType}_{orderForPdf.OrderDate:yyyy-MM-dd}.pdf";

            var message = new Message(new List<(string Name, string Address)> { (fullName, orderForPdf.Customer.Email) }, subject, messageText);

            await _emailSender.SendEmailAsync(message, pdfBytes, attachmentName);
        }

        private async Task CheckIsLabOpen(DateTime scheduledDateTime)
        {
            var labOpeningHours = await _appConfigService.GetLabConfigOpeningHours();

            if (!LabDailyAvailabilityProvider.IsWhenOfficeIsOpen(scheduledDateTime, labOpeningHours))
            {
                var dateTimeString = scheduledDateTime.ToString("yyyy-MM-dd HH:mm");
                throw new CustomException($"The Lab is Closed on '{dateTimeString}'");
            }
        }
    }
}
