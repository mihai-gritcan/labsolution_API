using LabSolution.HttpModels;
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
    public class AvailabilityController : ControllerBase
    {

        private readonly IOrderService _orderService;

        public AvailabilityController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetAvailableTimeSlots([FromQuery] DailySlotsAvailabilityRequest dailySlotsAvailabilityRequest)
        {
            if (!LabDailyAvailabilityProvider.IsWhenOfficeIsOpen(dailySlotsAvailabilityRequest.Date))
                return Ok(new DailyAvailableTimeSlotsResponse(dailySlotsAvailabilityRequest.Date));

            var placedOrders = await _orderService.GetOccupiedTimeSlots(dailySlotsAvailabilityRequest.Date);

            return Ok(BuildDailyStructure(dailySlotsAvailabilityRequest.Date, placedOrders));
        }

        private static DailyAvailableTimeSlotsResponse BuildDailyStructure(DateTime date, List<DateTime> occupiedSlots)
        {
            var structure = new DailyAvailableTimeSlotsResponse(date);

            var iterator = LabDailyAvailabilityProvider.StartOfDay(date);

            while (iterator < LabDailyAvailabilityProvider.EndOfDay(date))
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
            return LabDailyAvailabilityProvider.DefaultPlacesPer10Minutes - placedOrdersCount;
        }
    }
}
