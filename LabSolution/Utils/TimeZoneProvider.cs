using System;
using System.Runtime.InteropServices;

namespace LabSolution.Utils
{
    public static class TimeZoneProvider
    {
        public static DateTime ToBucharestTimeZone(this DateTime date)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(date, GetTimeZoneInfo());
        }

        private static TimeZoneInfo GetTimeZoneInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest");
            }

            throw new NotImplementedException("I don't know how to do a lookup on a Mac.");
        }
    }
}
