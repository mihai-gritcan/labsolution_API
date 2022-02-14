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
        public async Task<ActionResult<DailyAvailableTimeSlotsResponse>> GetAvailableTimeSlots([FromQuery] DailySlotsAvailabilityRequest dailySlotsAvailabilityRequest)
        {
            var labConfigPersonsAndIntervals = await _appConfigService.GetLabConfigPersonsAndIntervals();
            var openingHours = await _appConfigService.GetOpeningHours();

            if (!LabDailyAvailabilityProvider.IsWorkingDay2(dailySlotsAvailabilityRequest.Date, openingHours))
                return Ok(new DailyAvailableTimeSlotsResponse(dailySlotsAvailabilityRequest.Date));

            var placedOrders = await _orderService.GetOccupiedTimeSlots(dailySlotsAvailabilityRequest.Date);

            return Ok(BuildDailyStructure(dailySlotsAvailabilityRequest.Date, placedOrders, openingHours, labConfigPersonsAndIntervals));
        }

        private DailyAvailableTimeSlotsResponse BuildDailyStructure(DateTime date, List<DateTime> occupiedSlots, List<OpeningHoursDto> openingHours, LabConfigPersonsAndIntervals labConfig)
        {
            var structure = new DailyAvailableTimeSlotsResponse(date);

            DateTime currentLocalTime = GetCurrentTimeNormalized();

            if (date.Date < currentLocalTime.Date) return structure;

            DateTime iterator;
            if (date.Date == currentLocalTime.Date)
            {
                var startOfDay = LabDailyAvailabilityProvider.GetStartOfDay2(date, openingHours);
                iterator = currentLocalTime > startOfDay ? currentLocalTime : startOfDay;
            }
            else
            {
                iterator = LabDailyAvailabilityProvider.GetStartOfDay2(date, openingHours);
            }

            while (iterator < LabDailyAvailabilityProvider.GetEndOfDay2(date, openingHours))
            {
                var nextIntervalStart = iterator.AddMinutes(labConfig.IntervalDurationMinutes);
                var numberOfSlots = GetNumberOfAvaliableSlotsPerInterval(iterator, nextIntervalStart, occupiedSlots, labConfig.PersonsInInterval);
                structure.AvailableSlots.Add(new DailyAvailableTimeSlotsResponse.TimeSlot(iterator, numberOfSlots));
                iterator = nextIntervalStart;
            }

            return structure;
        }

        private static int GetNumberOfAvaliableSlotsPerInterval(DateTime intervalStart, DateTime intervalEnd, List<DateTime> dailyOccupiedSlots, int acceptedPersonsInInterval)
        {
            var placedOrdersCount = dailyOccupiedSlots.Count(x => x >= intervalStart && x < intervalEnd);
            return acceptedPersonsInInterval - placedOrdersCount > 0 ? acceptedPersonsInInterval - placedOrdersCount : 0;
        }

        private static DateTime GetCurrentTimeNormalized()
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
