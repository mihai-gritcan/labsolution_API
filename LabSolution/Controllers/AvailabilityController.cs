using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly LabOpeningHoursOptions _openingHoursOptions;

        public AvailabilityController(IOrderService orderService, IOptions<LabOpeningHoursOptions> options)
        {
            _orderService = orderService;
            _openingHoursOptions = options.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetAvailableTimeSlots([FromQuery] DailySlotsAvailabilityRequest dailySlotsAvailabilityRequest)
        {
            if (!LabDailyAvailabilityProvider.IsWorkingDay(dailySlotsAvailabilityRequest.Date, _openingHoursOptions))
                return Ok(new DailyAvailableTimeSlotsResponse(dailySlotsAvailabilityRequest.Date));

            var placedOrders = await _orderService.GetOccupiedTimeSlots(dailySlotsAvailabilityRequest.Date);

            return Ok(BuildDailyStructure(dailySlotsAvailabilityRequest.Date, placedOrders));
        }

        private DailyAvailableTimeSlotsResponse BuildDailyStructure(DateTime date, List<DateTime> occupiedSlots)
        {
            var structure = new DailyAvailableTimeSlotsResponse(date);

            DateTime currentLocalTime = GetCurrentTimeNormalized();
            
            var iterator = date.Date == currentLocalTime.Date ? currentLocalTime : LabDailyAvailabilityProvider.GetStartOfDay(date, _openingHoursOptions);

            while (iterator < LabDailyAvailabilityProvider.GetEndOfDay(date, _openingHoursOptions))
            {
                var nextIntervalStart = iterator.AddMinutes(_openingHoursOptions.IntervalDurationMinutes);
                structure.AvailableSlots.Add(new DailyAvailableTimeSlotsResponse.TimeSlot(iterator, GetNumberOfAvaliableSlotsPerInterval(iterator, nextIntervalStart, occupiedSlots)));
                iterator = nextIntervalStart;
            }

            return structure;
        }

        private int GetNumberOfAvaliableSlotsPerInterval(DateTime intervalStart, DateTime intervalEnd, List<DateTime> dailyOccupiedSlots)
        {
            var placedOrdersCount = dailyOccupiedSlots.Count(x => x >= intervalStart && x < intervalEnd);
            return _openingHoursOptions.PersonsInInterval - placedOrdersCount;
        }

        private DateTime GetCurrentTimeNormalized()
        {
            DateTime currentLocalTime = DateTime.UtcNow.ToBucharestTimeZone();

            while(currentLocalTime.Minute % 5 != 0)
            {
                currentLocalTime = currentLocalTime.AddMinutes(1);
            }
            return currentLocalTime;
        }
    }
}
