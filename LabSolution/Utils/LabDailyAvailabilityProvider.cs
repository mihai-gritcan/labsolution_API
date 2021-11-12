using LabSolution.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;

namespace LabSolution.Utils
{
    public static class LabDailyAvailabilityProvider
    {
        private static LabOpeningHoursOptions OpeningHoursOptions = new LabOpeningHoursOptions();

        private static DateTime StartOfDay(DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, OpeningHoursOptions.StartDayHour, OpeningHoursOptions.StartDayMinutes, 0);

        private static DateTime EndOfDay(DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, OpeningHoursOptions.EndDayHour, OpeningHoursOptions.EndDayMinutes, 0);

        public static DateTime GetStartOfDay(DateTime date, LabOpeningHoursOptions openingHoursOptions)
        {
            OpeningHoursOptions = openingHoursOptions;
            return StartOfDay(date);
        }

        public static DateTime GetEndOfDay(DateTime date, LabOpeningHoursOptions openingHoursOptions)
        {
            OpeningHoursOptions = openingHoursOptions;
            return EndOfDay(date);
        }

        public static bool IsWhenOfficeIsOpen(DateTime date)
        {
            Startup.StaticConfig.GetSection("LabOpeningHoursOptions").Bind(OpeningHoursOptions, c => c.BindNonPublicProperties = true);

            return IsWorkingDay(date) && date >= StartOfDay(date) && date < EndOfDay(date);
        }

        public static bool IsWorkingDay(DateTime date, LabOpeningHoursOptions openingHoursOptions)
        {
            OpeningHoursOptions = openingHoursOptions;
            return IsWorkingDay(date);
        }

        private static bool IsWorkingDay(DateTime date)
        {
            return OpeningHoursOptions.WorkingDays.Contains(date.DayOfWeek.ToString());
        }
    }
}
