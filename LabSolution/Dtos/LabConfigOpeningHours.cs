using System.Collections.Generic;

namespace LabSolution.Dtos
{
    public class LabConfigOpeningHours
    {
        public string StartDayTime { get; set; }
        public string EndDayTime { get; set; }
        public int IntervalDurationMinutes { get; set; }
        public int PersonsInInterval { get; set; }
        public List<string> WorkingDays { get; set; }

        public int StartDayHour => int.Parse(StartDayTime.Split(":")[0]);
        public int StartDayMinutes => int.Parse(StartDayTime.Split(":")[1]);

        public int EndDayHour => int.Parse(EndDayTime.Split(":")[0]);
        public int EndDayMinutes => int.Parse(EndDayTime.Split(":")[1]);
    }
}
