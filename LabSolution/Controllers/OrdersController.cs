using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace LabSolution.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IConverter _converter;

        public OrdersController(ICustomerService customerService, IOrderService orderService, IConverter converter)
        {
            _customerService = customerService;
            _orderService = orderService;
            _converter = converter;
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
            var allSavedCustomers = await _customerService.SaveCustomers(createOrder.Customers);

            var addedOrders = await _orderService.SaveOrders(createOrder, allSavedCustomers);

            return addedOrders.Select(x => new CreatedOrdersResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                Customer = CustomerDto.CreateDtoFromEntity(allSavedCustomers.Find(c => c.Id == x.CustomerId), isRootCustomer: x.ParentId == null),
                ParentId = x.ParentId,
                PlacedAt = x.PlacedAt,
                Scheduled = x.Scheduled,
                TestLanguage = (TestLanguage)x.TestLanguage,
                TestType = (TestType)x.TestType,
            });
        }

        // reception getCreatedOrders ByDate or idnp
        [HttpGet("{date}/created")]
        public async Task<ActionResult> GetCreatedOrders(DateTime date, [FromQuery] long? idnp)
        {
            return Ok(await _orderService.GetCreatedOrders(date, idnp));
        }

        // reception getFinishedOrders ByDate or idnp
        [HttpGet("{date}/finished")]
        public async Task<ActionResult<object>> GetFinishedOrders(DateTime date, [FromQuery] long? idnp)
        {
            return Ok(await _orderService.GetFinishedOrders(date, idnp));
        }

        // reception updateOrder 
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

        // reception start printing the command confirmation
        [HttpPatch("{id}/processing-ticket")]
        public async Task<ActionResult<ProcessedOrderResponse>> GetProcessingTicket(int id)
        {
            var orderDetails = await _orderService.GetOrderDetails(id);
            if (orderDetails is null)
                return NotFound();

            var savedProcessedOrder = await _orderService.CreateOrUpdateProcessedOrder(id);
            var numericCode7Digicts = savedProcessedOrder.Id.ToString("D7");
            var barcode = BarcodeProvider.GenerateBarcodeFromNumericCode(numericCode7Digicts);

            return Ok(new ProcessedOrderResponse
            {
                Id = savedProcessedOrder.Id,
                CustomerOrderId = savedProcessedOrder.CustomerOrderId,
                NumericCode = numericCode7Digicts,
                Barcode = barcode,
                ProcessedAt = savedProcessedOrder.ProcessedAt,
                TestLanguage = (TestLanguage)orderDetails.TestLanguage,
                TestType = (TestType)orderDetails.TestType,
                Customer = CustomerDto.CreateDtoFromEntity(orderDetails.Customer, orderDetails.ParentId == null)
            });
        }

        // reception set test result by processedOrderId
        [HttpPatch("{processedOrderId}/settestresult")]
        public async Task<IActionResult> SetTestResult(int processedOrderId, SetTestResultRequest setResultRequest)
        {
            if (processedOrderId != setResultRequest.ProcesedOrderId)
                return BadRequest();

            await _orderService.SetTestResult(setResultRequest.ProcesedOrderId, setResultRequest.TestResult, setResultRequest.ExecutorName, setResultRequest.VerifierName, setResultRequest.ValidatorName);

            return NoContent();
        }

        // reception getPdfResult by processedOrderId
        [HttpGet("{id}/pdfresult")]
        public async Task<IActionResult> GetPdfResuly(int id)
        {
            var res = await _orderService.GetFinishedOrderForPdf(id);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
                Out = Path.Combine(Directory.GetCurrentDirectory(), "AvtoGenerated", $"Employee_Report{res.Id}.pdf")
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = TemplateGenerator.GetHTMLString(),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            _converter.Convert(pdf);
            return Ok("Successfully created PDF document.");
        }
    }

    public class Employee
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }
    public static class DataStorage
    {
        public static List<Employee> GetAllEmployees() =>
            new List<Employee>
            {
                new Employee { Name="Mike", LastName="Turner", Age=35, Gender="Male"},
                new Employee { Name="Sonja", LastName="Markus", Age=22, Gender="Female"},
                new Employee { Name="Luck", LastName="Martins", Age=40, Gender="Male"},
                new Employee { Name="Sofia", LastName="Packner", Age=30, Gender="Female"},
                new Employee { Name="John", LastName="Doe", Age=45, Gender="Male"}
            };
    }

    public static class TemplateGenerator
    {
        public static string GetHTMLString()
        {
            var employees = DataStorage.GetAllEmployees();
            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'><h1>This is the generated PDF report!!!</h1></div>
                                <table align='center'>
                                    <tr>
                                        <th>Name</th>
                                        <th>LastName</th>
                                        <th>Age</th>
                                        <th>Gender</th>
                                    </tr>");
            foreach (var emp in employees)
            {
                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                  </tr>", emp.Name, emp.LastName, emp.Age, emp.Gender);
            }
            sb.Append(@"
                                </table>
                            </body>
                        </html>");
            return sb.ToString();
        }
    }
}
