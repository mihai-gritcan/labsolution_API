using LabSolution.HttpModels;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodesController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public CodesController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("numericCode")]
        public async Task<ActionResult> GetNumericCode([FromQuery] int orderId)
        {
            var orderScheduleDateTime = await _orderService.GetOrderScheduledDateTime(orderId);
            return Ok(NumericCodeProvider.GenerateNumericCode(orderScheduleDateTime, orderId));
        }

        [HttpGet("barCode")]
        public async Task<ActionResult> GetBarCode([FromQuery] int orderId)
        {
            var orderScheduleDateTime = await _orderService.GetOrderScheduledDateTime(orderId);
            var numericCode = NumericCodeProvider.GenerateNumericCode(orderScheduleDateTime, orderId);

            return Ok();
        }
    }
}
