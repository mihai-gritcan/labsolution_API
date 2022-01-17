using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSolution.Models;
using System.Collections.Generic;
using LabSolution.Infrastructure;
using Microsoft.Extensions.Options;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GovSyncController : BaseApiController
    {
        private readonly LabSolutionContext _context;
        private readonly GovSyncConfiguration _govSyncConfiguration;

        public GovSyncController(LabSolutionContext context, IOptions<GovSyncConfiguration> options)
        {
            _context = context;
            _govSyncConfiguration = options.Value;
        }

        [HttpPatch("sync")]
        public async Task<IActionResult> SyncOrdersToGov([FromBody] OrdersToSync ordersToSync)
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
                        LaboratoryId = "LaboratoryId123", //  TODO: get it from MedTest
                        LaboratoryOfficeId = "LaboratoryOfficeId123", //  TODO: get it from MedTest
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

    public class OrdersToSync
    {
        /// <summary> The list of Processed OrdersIds to be synched with Gov </summary>
        public List<int> ProcessedOrderIds { get; set; }
    }
}
