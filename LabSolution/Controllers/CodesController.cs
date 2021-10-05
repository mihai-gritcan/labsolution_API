using BarcodeLib;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null) return NotFound();

            return Ok(NumericCodeProvider.GenerateNumericCode(orderDetails.Scheduled, orderId));
        }

        [HttpGet("barCode")]
        public async Task<ActionResult> GetBarCode([FromQuery] int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails is null) return NotFound();

            var numericCode = NumericCodeProvider.GenerateNumericCode(orderDetails.Scheduled, orderId);

            var barcode = new Barcode();
            var img = barcode.Encode(TYPE.CODE39, numericCode.ToString(), Color.Black, Color.White, 250, 100);
            var data = ConvertImageToBytes(img);
            return Ok(File(data, "image/jpeg"));
        }

        private static byte[] ConvertImageToBytes(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
