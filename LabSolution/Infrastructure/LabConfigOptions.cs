using System.Collections.Generic;

namespace LabSolution.Infrastructure
{
    public class LabConfigOptions
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Site { get; set; }
    }

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
