using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSolution.Models;
using LabSolution.Infrastructure;
using Microsoft.Extensions.Options;
using LabSolution.HttpModels;
using Microsoft.AspNetCore.Authorization;
using LabSolution.Services;
using LabSolution.Clients;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GovSyncController : BaseApiController
    {
        private readonly LabSolutionContext _context;
        private readonly GovSyncConfiguration _govSyncConfiguration;

        private readonly IHttpClientServiceImplementation _httpClientService;
        private readonly GovSyncClient _govSyncClient;

        public GovSyncController(LabSolutionContext context, IOptions<GovSyncConfiguration> options, IHttpClientServiceImplementation httpClientService, GovSyncClient govSyncClient)
        {
            _context = context;
            _govSyncConfiguration = options.Value;
            _httpClientService = httpClientService;
            _govSyncClient = govSyncClient;
        }

        [AllowAnonymous]
        [HttpGet("1")]
        public async Task<IActionResult> SimulateHttpCall1()
        {
            return Ok(await _httpClientService.Execute1());
        }

        [AllowAnonymous]
        [HttpGet("2")]
        public async Task<IActionResult> SimulateHttpCall2()
        {
            return Ok(await _httpClientService.Execute2());
        }

        [AllowAnonymous]
        [HttpGet("3")]
        public async Task<IActionResult> SimulateHttpCall3()
        {
            return Ok(await _govSyncClient.GetCompanies());
        }

        [HttpPatch("sync")]
        public async Task<IActionResult> SyncOrdersToGov([FromBody] OrdersToSyncRequest ordersToSync)
        {
            if (!_govSyncConfiguration.IsSyncToGovEnabled)
                return BadRequest("Synchronization with Gov is not enabled. Please enable the option and retry");

            var ordersFromDb = await _context.ProcessedOrders.Where(x => ordersToSync.ProcessedOrderIds.Contains(x.Id))
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Select(x => new
                {
                    Customer = new {
                        IsResident = true, // TODO: get it from MedTest
                        x.CustomerOrder.Customer.PersonalNumber,
                        x.CustomerOrder.Customer.FirstName,
                        x.CustomerOrder.Customer.LastName,
                        x.CustomerOrder.Customer.DateOfBirth,
                        x.CustomerOrder.Customer.Passport,
                        x.CustomerOrder.Customer.Gender,
                        x.CustomerOrder.Customer.Address
                    },
                    Sample = new
                    {
                        LaboratoryId = _govSyncConfiguration.LaboratoryId,
                        LaboratoryOfficeId = _govSyncConfiguration.LaboratoryOfficeId,
                        LaboratoryTestNumber = x.Id,
                        SampleType = x.CustomerOrder.TestType,
                        SampleCollectionAt = x.ProcessedAt,
                        SampleResult = x.TestResult
                    },
                    CaseStartDate = x.ProcessedAt // should be the Start of a Positive test or the Date when the sample was collected
                })
                .ToListAsync();

            // TODO: call HttpClient to send the orders to Government

            return NoContent();
        }
    }
}
