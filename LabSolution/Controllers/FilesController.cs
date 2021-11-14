using LabSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : BaseApiController
    {
        private readonly IOrderService _orderService;

        private readonly ILogger<OrdersController> _logger;

        public FilesController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [AllowAnonymous]
        // public serve existing pdf 
        [HttpGet("{pdfNameHex}")]
        public async Task<IActionResult> GetPdfResultFromDb(string pdfNameHex)
        {
            var existingPdf = await _orderService.GetPdfBytes(pdfNameHex);

            if (existingPdf is null) return NotFound("Can't locate the requested PDF");

            MemoryStream stream = new MemoryStream(existingPdf.PdfBytes);
            return new FileStreamResult(stream, "application/pdf");
        }
    }
}
