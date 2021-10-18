using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodesController : BaseApiController
    {
        private readonly IOrderService _orderService;
        public CodesController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("numericCode")]
        public async Task<ActionResult> GetNumericCode([FromQuery] int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null) return NotFound();

            return Ok(BarcodeProvider.GenerateNumericCode(orderDetails.Scheduled, orderId));
        }

        [HttpGet("barcode")]
        public async Task<ActionResult> GetBarCode([FromQuery] int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null) return NotFound();

            var barcode = BarcodeProvider.GenerateBarcode(orderDetails.Scheduled, orderId);

            return Ok(File(barcode, "image/jpeg"));
        }
        
        [HttpGet("qrcode")]
        public async Task<ActionResult> GetQRCode([FromQuery] int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null) return NotFound();

            var numericCode = BarcodeProvider.GenerateNumericCode(orderDetails.Scheduled, orderId);
            var qrCode = QRCodeProvider.GeneratQRCode(numericCode);

            return Ok(File(qrCode, "image/jpeg"));
        }
    }
}
