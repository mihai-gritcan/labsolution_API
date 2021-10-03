using System;
using System.Collections.Generic;

namespace LabSolution.Controllers
{
    public class DailyAvailableTimeSlotsResponse
    {
        public DailyAvailableTimeSlotsResponse(DateTime date)
        {
            Date = date.Date.ToShortDateString();
            AvailableSlots = new List<TimeSlot>();
        }
        public string Date { get; set; }
        public List<TimeSlot> AvailableSlots { get; set; }

        public class TimeSlot
        {
            public TimeSlot(DateTime date, int numberOfSlots)
            {
                Time = $"{date.Hour}:{date.Minute}";
                NumberOfSlots = numberOfSlots;
            }

            public string Time { get; set; }
            public int NumberOfSlots { get; set; }
        }
    }
}
