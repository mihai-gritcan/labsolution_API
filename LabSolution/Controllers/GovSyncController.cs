using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSolution.Models;
using LabSolution.Infrastructure;
using Microsoft.Extensions.Options;
using LabSolution.HttpModels;
using LabSolution.GovSync;
using LabSolution.Enums;
using System.Collections.Generic;
using LabSolution.Utils;
using System;
using Microsoft.Extensions.Logging;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GovSyncController : BaseApiController
    {
        private readonly LabSolutionContext _context;
        private readonly GovSyncConfiguration _govSyncConfiguration;

        private readonly GovSyncClient _govSyncClient;

        private readonly ILogger<GovSyncController> _logger;

        public GovSyncController(LabSolutionContext context, IOptions<GovSyncConfiguration> options, GovSyncClient govSyncClient, ILogger<GovSyncController> logger)
        {
            _context = context;
            _govSyncConfiguration = options.Value;
            _govSyncClient = govSyncClient;

            _logger = logger;
        }

        // TODO: #54 - Move this method into a CronJob process
        private async Task RemovePdfsOlderThanXDays()
        {
            // silent execute this action without throwing (when finish implementing #54 get rid of silency)
            try
            {
                var numberOfDays = Startup.StaticConfig["RemovePdfsOlderThanDays"];
                var days = int.Parse(numberOfDays);

                var today = DateTime.UtcNow.Date.ToBucharestTimeZone();
                var checkPointDate = today.AddDays(-1 * days);

                _context.ProcessedOrderPdfs.RemoveRange(_context.ProcessedOrderPdfs.Where(x => x.DateCreated.Date < checkPointDate));
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        [HttpPatch("sync")]
        public async Task<ActionResult<SyncResponse>> SyncOrdersToGov([FromBody] OrdersToSyncRequest ordersToSync)
        {
            if (!_govSyncConfiguration.IsSyncToGovEnabled)
                return BadRequest("Synchronization with Gov is not enabled. Please enable the option and retry");

            const string nonExistentPersonalNumber = "-";

            var orders = await _context.ProcessedOrders.Include(x => x.CustomerOrder)
                .Where(x => ordersToSync.ProcessedOrderIds.Contains(x.Id)
                        && (x.CustomerOrder.TestType == (short)TestType.Antigen || x.CustomerOrder.TestType == (short)TestType.PCR || x.CustomerOrder.TestType == (short)TestType.PCRExpress)) // Gov supports only PCR and Antigen tests
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Select(x => new TestPushModel
                {
                    PersonInfo = new PersonInfoModel {
                        IsResident = true, // TODO: get it from MedTest
                        IdentityNumber = !string.IsNullOrWhiteSpace(x.CustomerOrder.Customer.PersonalNumber) ? x.CustomerOrder.Customer.PersonalNumber : nonExistentPersonalNumber,
                        FirstName = x.CustomerOrder.Customer.FirstName,
                        LastName = x.CustomerOrder.Customer.LastName,
                        BirthDay = x.CustomerOrder.Customer.DateOfBirth,
                        PhoneNumber = x.CustomerOrder.Customer.Phone,
                        Gender = x.CustomerOrder.Customer.Gender == (int)Gender.Male ? "Male" : "Female",
                        Address = new AddressModel { Municipality = x.CustomerOrder.Customer.Address },
                        WorkingInfo = new WorkingInfoModel { Position = string.Empty }
                    },
                    SampleInfo = new SampleInfoModel
                    {
                        LaboratoryId = _govSyncConfiguration.LaboratoryId,
                        LaboratoryOfficeId = _govSyncConfiguration.LaboratoryOfficeId,
                        LaboratoryTestNumber = x.Id.ToString("D7"),
                        SampleType = x.CustomerOrder.TestType == (short)TestType.PCR || x.CustomerOrder.TestType == (short)TestType.PCRExpress ? "PCR" : "AntiGen",
                        SampleCollectionAt = x.ProcessedAt,
                        SampleResult = x.TestResult == (int)TestResult.Positive ? "Positive" : "Negative",
                        TestDeviceIdentifier = GetTestDeviceIdentifier((TestType)x.CustomerOrder.TestType)
                    },
                    VaccinationInfo = new VaccinationInfoModel(),
                    CaseStartDate = x.ProcessedAt // should be the Start of a Positive test or the Date when the sample was collected
                })
                .ToListAsync();

            var syncResult = await _govSyncClient.SendTestResults(orders);

            await SaveSynchedOrders(syncResult.SynchedItems);

            await RemovePdfsOlderThanXDays();

            return Accepted(new SyncResponse(syncResult));
        }

        private string GetTestDeviceIdentifier(TestType testType)
        {
            // Ex: Pentru dispozitivul "SD BIOSENSOR Inc, STANDARD F COVID-19 Ag FIA", câmpul se va completa cu valoarea "344"
            return testType == TestType.Antigen ? _govSyncConfiguration.LaboratoryAntigenDeviceIdentifier : null;
        }

        private async Task SaveSynchedOrders(List<TestPushModel> synchedTests)
        {
            var date = DateTime.UtcNow.ToBucharestTimeZone();
            var entitiesToSave = synchedTests.Select(x => new OrderSyncToGov
            {
                DateSynched = date,
                ProcessedOrderId = int.Parse(x.SampleInfo.LaboratoryTestNumber),
                TestResultSyncStatus = x.SampleInfo.SampleResult == nameof(TestResult.Positive)
            });

            _context.OrdersSyncToGov.AddRange(entitiesToSave);
            await _context.SaveChangesAsync();
        }
    }

    public record SyncResponse
    {
        public SyncResponse(SyncResultDto syncResult)
        {
            SynchedItems = syncResult.SynchedItems.ConvertAll(x => int.Parse(x.SampleInfo.LaboratoryTestNumber));
            UnsynchedItems = syncResult.UnsynchedItems.ConvertAll(x => new KeyValuePair<int, string>(int.Parse(x.Key.SampleInfo.LaboratoryTestNumber), x.Value));
        }

        public List<int> SynchedItems { get; set; }
        public List<KeyValuePair<int, string>> UnsynchedItems { get; set; }
    }
}
