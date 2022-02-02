using LabSolution.Dtos;
using LabSolution.Enums;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Notifications;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

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

        private readonly INotificationManager _notificationManager;

        private readonly AppEmailNotificationConfig _appEmailNotificationConfig;

        public OrdersController(ICustomerService customerService, IOrderService orderService,
            IPdfReportProvider pdfReportProvider, ILogger<OrdersController> logger,
            IAppConfigService appConfigService, INotificationManager notificationManager,
            IOptions<AppEmailNotificationConfig> options)
        {
            _customerService = customerService;
            _orderService = orderService;
            _pdfReportProvider = pdfReportProvider;
            _logger = logger;
            _appConfigService = appConfigService;
            _notificationManager = notificationManager;

            _appEmailNotificationConfig = options.Value;
        }

        // public order submit
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<CreatedOrdersResponse>>> CreateOrder([FromBody] CreateOrderRequest createOrder)
        {
            var savedOrders = await SaveOrder(createOrder);

            if (_appEmailNotificationConfig.SendNotificationForOnlineBooking)
            {
                var labConfigs = await _appConfigService.GetLabConfigAddresses();
                await _notificationManager.NotifyOrdersCreated(savedOrders, labConfigs);
            }

            return Ok(savedOrders);
        }

        // reception order submit
        [HttpPost("elevated")]
        public async Task<ActionResult<IEnumerable<CreatedOrdersResponse>>> CreateElevatedOrder([FromBody] CreateOrderRequest createOrder)
        {
            var savedOrders = await SaveOrder(createOrder);
            if (_appEmailNotificationConfig.SendNotificationForInHouseBooking)
            {
                var labConfigs = await _appConfigService.GetLabConfigAddresses();
                await _notificationManager.NotifyOrdersCreated(savedOrders, labConfigs);
            }

            return Ok(savedOrders);
        }

        // reception getOrders ByDate or idnp (can include Gov sync state data)
        [HttpGet("{date}")]
        public async Task<ActionResult<List<OrderWithStatusResponse>>> GetOrders(DateTime date, [FromQuery] string idnp, [FromQuery] bool includeSyncState = false)
        {
            return Ok(await _orderService.GetOrdersWithStatus(date, idnp, includeSyncState));
        }

        // reception getOrders in range Date
        [HttpGet]
        public async Task<ActionResult<List<OrderWithStatusResponse>>> GetOrders([FromQuery][Required] DateTime start, [FromQuery][Required] DateTime end, [FromQuery] TestType? type = null)
        {
            return Ok(await _orderService.GetOrdersWithStatus(start, end, type));
        }

        // reception getPriceStatistics in range Date
        [HttpGet("price-statistics")]
        public async Task<ActionResult<PriceStatisticsDto>> GetPriceStatistics([FromQuery][Required] DateTime start, [FromQuery][Required] DateTime end)
        {
            EnsureSuperUserPerformsTheAction();

            return Ok(await _orderService.GetPriceStatistics(start, end));
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
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] UpdateOrderRequest updateOrderRequest)
        {
            if (orderId != updateOrderRequest.Id)
                return BadRequest();

            await CheckIsLabOpen(updateOrderRequest.ScheduledDateTime);

            await _orderService.UpdateOrder(updateOrderRequest);

            return NoContent();
        }

        // reception wants to print the order
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

        // reception confirms printing the order with specified price
        [HttpPatch("{processedOrderId}/confirm-processing-ticket")]
        public async Task<IActionResult> ConfirmProcessingTicket(int processedOrderId, [FromBody]ProcessedOrderSetPriceRequest setPriceRequest)
        {
            if (setPriceRequest.ProcessedOrderId != processedOrderId)
                return BadRequest();

            await _orderService.SetTestPrice(processedOrderId, setPriceRequest.Price);
            return NoContent();
        }

        [HttpGet("{date}/processed")]
        public async Task<ActionResult<List<ProcessedOrderToSetResultResponse>>> GetOrdersToSetResult(DateTime date, [FromQuery] string numericCode)
        {
            return Ok(await _orderService.GetOrdersToSetResult(date, numericCode));
        }

        // reception set test result by processedOrderId
        [HttpPatch("{processedOrderId}/settestresult")]
        public async Task<IActionResult> SetTestResult(int processedOrderId, [FromBody] SetTestResultRequest setResultRequest)
        {
            if (processedOrderId != setResultRequest.ProcessedOrderId)
                return BadRequest();

            var labConfigs = await _appConfigService.GetLabConfigAddresses();
            var fileName = $"{Guid.NewGuid()}";

            var processedOrderForPdf = await _orderService.SetTestResult(setResultRequest.ProcessedOrderId, setResultRequest.TestResult,
                setResultRequest.ExecutorName, setResultRequest.VerifierName, setResultRequest.ValidatorName, setResultRequest.AntibodyUnits);

            var pdfBytes = await _pdfReportProvider.CreatePdfReport(fileName, processedOrderForPdf, labConfigs);

            await _orderService.SaveOrReplacePdfBytes(processedOrderForPdf.OrderId, fileName, pdfBytes);

            if (pdfBytes is null)
                return BadRequest("Something went wrong during PDF creation. Please retry");

            if(_appEmailNotificationConfig.SendNotificationWhenTestIsCompleted && !string.IsNullOrWhiteSpace(processedOrderForPdf.Customer.Email))
                await _notificationManager.NotifyOrderCompleted(processedOrderForPdf, labConfigs, pdfBytes);

            return NoContent();
        }

        private async Task<IEnumerable<CreatedOrdersResponse>> SaveOrder(CreateOrderRequest createOrder)
        {
            await CheckIsLabOpen(createOrder.ScheduledDateTime);

            var savedOrders = new List<CreatedOrdersResponse>();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var allSavedCustomers = await _customerService.CreateCustomers(createOrder.Customers);

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
