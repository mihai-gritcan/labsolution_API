using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
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
                Customer = CustomerDto.CreateDtoFromEntity(allSavedCustomers.Find(c => c.Id == x.CustomerId)),
                ParentId = x.ParentId,
                Placed = x.Placed,
                Scheduled = x.Scheduled,
                PrefferedLanguage = (TestLanguage)x.PrefferedLanguage,
                TestType = (TestType)x.TestType
            });

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult> GetOrdersByDate([FromQuery] DateTime date)
        {
            return Ok(await _orderService.GetOrders(date));
        }
    }
}
