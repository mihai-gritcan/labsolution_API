using LabSolution.HttpModels;
using LabSolution.Services;
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
    public class AvailabilityController : ControllerBase
    {
        private const int _defaultStartHour = 8;
        private const int _defaultEndHour = 18;
        private const int _defaultPlacesPer10Minutes = 5;

        private readonly IOrderService _orderService;

        public AvailabilityController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Default configuration is from 8:00 to 18:00 -> 5 places at each 10 minutes

        private DateTime StartOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day, _defaultStartHour, 0, 0);
        private DateTime EndOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day, _defaultEndHour, 0, 0);

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetAvailableTimeSlots([FromQuery] DailySlotsAvailabilityRequest dailySlotsAvailabilityRequest)
        {
            if (dailySlotsAvailabilityRequest.Date.DayOfWeek == DayOfWeek.Sunday)
                return Ok(new DailyAvailableTimeSlotsResponse(dailySlotsAvailabilityRequest.Date));

            var placedOrders = await _orderService.GetOccupiedTimeSlots(dailySlotsAvailabilityRequest.Date);

            return Ok(BuildDailyStructure(dailySlotsAvailabilityRequest.Date, placedOrders));
        }

        private DailyAvailableTimeSlotsResponse BuildDailyStructure(DateTime date, List<DateTime> occupiedSlots)
        {
            var structure = new DailyAvailableTimeSlotsResponse(date);

            var iterator = StartOfDay(date);

            while (iterator < EndOfDay(date))
            {
                var nextIntervalStart = iterator.AddMinutes(10);
                structure.AvailableSlots.Add(new DailyAvailableTimeSlotsResponse.TimeSlot(iterator, GetNumberOfAvaliableSlotsPerInterval(iterator, nextIntervalStart, occupiedSlots)));
                iterator = nextIntervalStart;
            }

            return structure;
        }

        private static int GetNumberOfAvaliableSlotsPerInterval(DateTime intervalStart, DateTime intervalEnd, List<DateTime> dailyOccupiedSlots)
        {
            var placedOrdersCount = dailyOccupiedSlots.Count(x => x >= intervalStart && x < intervalEnd);
            return _defaultPlacesPer10Minutes - placedOrdersCount;
        }
    }
}
