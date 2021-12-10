using LabSolution.HttpModels;
using LabSolution.Dtos;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IAppConfigService _appConfigService;

        public AvailabilityController(IOrderService orderService, IAppConfigService appConfigService)
        {
            _orderService = orderService;
            _appConfigService = appConfigService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetAvailableTimeSlots([FromQuery] DailySlotsAvailabilityRequest dailySlotsAvailabilityRequest)
        {
            var labOpeningHours = await _appConfigService.GetLabConfigOpeningHours();

            if (!LabDailyAvailabilityProvider.IsWorkingDay(dailySlotsAvailabilityRequest.Date, labOpeningHours))
                return Ok(new DailyAvailableTimeSlotsResponse(dailySlotsAvailabilityRequest.Date));

            var placedOrders = await _orderService.GetOccupiedTimeSlots(dailySlotsAvailabilityRequest.Date);

            return Ok(BuildDailyStructure(dailySlotsAvailabilityRequest.Date, placedOrders, labOpeningHours));
        }

        private DailyAvailableTimeSlotsResponse BuildDailyStructure(DateTime date, List<DateTime> occupiedSlots, LabConfigOpeningHours labOpeningHours)
        {
            var structure = new DailyAvailableTimeSlotsResponse(date);

            DateTime currentLocalTime = GetCurrentTimeNormalized();

            DateTime iterator;
            if (date.Date == currentLocalTime.Date)
            {
                if (currentLocalTime > LabDailyAvailabilityProvider.GetStartOfDay(date, labOpeningHours))
                {
                    iterator = currentLocalTime;
                }
                else
                {
                    iterator = LabDailyAvailabilityProvider.GetStartOfDay(date, labOpeningHours);
                }
            }
            else
            {
                iterator = LabDailyAvailabilityProvider.GetStartOfDay(date, labOpeningHours);
            }

            while (iterator < LabDailyAvailabilityProvider.GetEndOfDay(date, labOpeningHours))
            {
                var nextIntervalStart = iterator.AddMinutes(labOpeningHours.IntervalDurationMinutes);
                structure.AvailableSlots.Add(new DailyAvailableTimeSlotsResponse.TimeSlot(iterator, GetNumberOfAvaliableSlotsPerInterval(iterator, nextIntervalStart, occupiedSlots, labOpeningHours)));
                iterator = nextIntervalStart;
            }

            return structure;
        }

        private int GetNumberOfAvaliableSlotsPerInterval(DateTime intervalStart, DateTime intervalEnd, List<DateTime> dailyOccupiedSlots, LabConfigOpeningHours labOpeningHours)
        {
            var placedOrdersCount = dailyOccupiedSlots.Count(x => x >= intervalStart && x < intervalEnd);
            return labOpeningHours.PersonsInInterval - placedOrdersCount;
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
