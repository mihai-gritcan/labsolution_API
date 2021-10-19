using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Models;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var allSavedCustomers = await _customerService.SaveCustomers(createOrder.Customers);

            var addedOrders = await _orderService.SaveOrders(createOrder, allSavedCustomers);

            var response = addedOrders.Select(x => new CreatedOrdersResponse {
                Id = x.Id,
                CustomerId = x.CustomerId,
                Customer = CustomerDto.CreateDtoFromEntity(allSavedCustomers.Find(c => c.Id == x.CustomerId), isRootCustomer: x.ParentId == null),
                ParentId = x.ParentId,
                Placed = x.Placed,
                Scheduled = x.Scheduled,
                PrefferedLanguage = (TestLanguage)x.PrefferedLanguage,
                TestType = (TestType)x.TestType,
            });

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult> GetOrdersByDate([FromQuery] DateTime date)
        {
            return Ok(await _orderService.GetOrders(date));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, UpdateOrderRequest updateOrderRequest)
        {
            if (id != updateOrderRequest.Id)
            {
                return BadRequest();
            }

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

        [HttpPatch("{orderId}/processing")]
        public async Task<ActionResult<ProcessedOrderResponse>> ProcessCustomerOrder(int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null)
                return NotFound();

            var numericCode = BarcodeProvider.GenerateNumericCode(orderDetails.Scheduled, orderId);
            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(numericCode);

            var savedProcessedOrder = await _orderService.SaveProcessedOrder(orderId, numericCode, barcode);

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
    }

    public class ProcessedOrderResponse
    {
        public int Id { get; set; }
        public int CustomerOrderId { get; set; }
        public long NumericCode { get; set; }
        public byte[] Barcode { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
