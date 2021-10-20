using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Models;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public OrdersController(ICustomerService customerService, IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderRequest createOrder)
        {
            return Ok(await SaveOrder(createOrder));
        }

        [HttpPost("elevated")]
        public async Task<ActionResult> CreateElevatedOrder([FromBody] CreateOrderRequest createOrder)
        {
            return Ok(await SaveOrder(createOrder));
        }

        private async Task<IEnumerable<CreatedOrdersResponse>> SaveOrder(CreateOrderRequest createOrder)
        {
            var allSavedCustomers = await _customerService.SaveCustomers(createOrder.Customers);

            var addedOrders = await _orderService.SaveOrders(createOrder, allSavedCustomers);

            return addedOrders.Select(x => new CreatedOrdersResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                Customer = CustomerDto.CreateDtoFromEntity(allSavedCustomers.Find(c => c.Id == x.CustomerId), isRootCustomer: x.ParentId == null),
                ParentId = x.ParentId,
                Placed = x.Placed,
                Scheduled = x.Scheduled,
                PrefferedLanguage = (TestLanguage)x.PrefferedLanguage,
                TestType = (TestType)x.TestType,
            });
        }

        [HttpGet("{date}/created")]
        public async Task<ActionResult> GetCreatedOrders(DateTime date, [FromQuery] long? idnp)
        {
            return Ok(await _orderService.GetCreatedOrders(date, idnp));
        }

        [HttpGet("{date}/finished")]
        public async Task<ActionResult<object>> GetFinishedOrders(DateTime date)
        {
            return Ok(await _orderService.GetFinishedOrders(date));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, UpdateOrderRequest updateOrderRequest)
        {
            if (id != updateOrderRequest.Id)
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

        [HttpPatch("{id}/processing")]
        public async Task<ActionResult<ProcessedOrderResponse>> ProcessCustomerOrder(int id)
        {
            var orderDetails = await _orderService.GetOrderDetails(id);
            if (orderDetails is null)
                return NotFound();

            var numericCode = BarcodeProvider.GenerateNumericCode(orderDetails.Scheduled, id);
            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(numericCode);

            var savedProcessedOrder = await _orderService.SaveProcessedOrder(id, numericCode, barcode);

            // TODO: we should return a PDF here
            return Ok(new ProcessedOrderResponse
            {
                Id = savedProcessedOrder.Id,
                CustomerOrderId = savedProcessedOrder.CustomerOrderId,
                NumericCode = savedProcessedOrder.NumericCode,
                Barcode = savedProcessedOrder.Barcode,
                ProcessedAt = savedProcessedOrder.ProcessedAt
            });
        }

        [HttpPatch("{id}/settestresult")]
        public async Task<IActionResult> SetTestResult(int id, SetTestResultRequest setTestResultRequest)
        {
            if (id != setTestResultRequest.OrderId)
                return BadRequest();

            await _orderService.SetTestResult(setTestResultRequest.OrderId, setTestResultRequest.NumericCode, setTestResultRequest.TestResult);

            return NoContent();
        }
    }
}
