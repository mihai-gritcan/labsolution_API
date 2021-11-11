using System;

namespace LabSolution.Utils
{
    public static class TimeZoneProvider
    {
        private const string BucharestTimeZoneId = "GTB Standard Time";

        public static DateTime ToBucharestTimeZone(this DateTime date)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(BucharestTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(date, localTimeZone);
        }
    }
}
