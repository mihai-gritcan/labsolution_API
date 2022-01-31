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

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GovSyncController : BaseApiController
    {
        private readonly LabSolutionContext _context;
        private readonly GovSyncConfiguration _govSyncConfiguration;

        private readonly GovSyncClient _govSyncClient;

        public GovSyncController(LabSolutionContext context, IOptions<GovSyncConfiguration> options, GovSyncClient govSyncClient)
        {
            _context = context;
            _govSyncConfiguration = options.Value;
            _govSyncClient = govSyncClient;
        }

        [HttpPatch("sync")]
        public async Task<ActionResult<SyncResponse>> SyncOrdersToGov([FromBody] OrdersToSyncRequest ordersToSync)
        {
            if (!_govSyncConfiguration.IsSyncToGovEnabled)
                return BadRequest("Synchronization with Gov is not enabled. Please enable the option and retry");

            var orders = await _context.ProcessedOrders.Where(x => ordersToSync.ProcessedOrderIds.Contains(x.Id))
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Select(x => new TestPushModel
                {
                    PersonInfo = new PersonInfoModel {
                        IsResident = true, // TODO: get it from MedTest
                        IdentityNumber = x.CustomerOrder.Customer.PersonalNumber,
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
                        SampleType = x.CustomerOrder.TestType == (short)TestType.PCR ? "PCR" : "AntiGen",
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

            return Accepted(new SyncResponse(syncResult));
        }

        // TODO: get it from MedTest
        private static string GetTestDeviceIdentifier(TestType testType)
        {
            // Ex: Pentru dispozitivul "SD BIOSENSOR Inc, STANDARD F COVID-19 Ag FIA", câmpul se va completa cu valoarea "344"
            return testType == TestType.Antigen ? "344" : null;
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
