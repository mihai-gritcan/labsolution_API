using System.Collections.Generic;

namespace LabSolution.Infrastructure
{

    public class LabOpeningHoursOptions
    {
        public List<string> WorkingDays { get; set; }
        public int StartDayHour { get; set; }
        public int StartDayMinutes { get; set; }
        public int EndDayHour { get; set; }
        public int EndDayMinutes { get; set; }
        public int IntervalDurationMinutes { get; set; }
        public int PersonsInInterval { get; set; }
    }
}
