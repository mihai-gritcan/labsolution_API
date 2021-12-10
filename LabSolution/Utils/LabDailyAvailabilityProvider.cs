using LabSolution.Dtos;
using System;

namespace LabSolution.Utils
{
    public static class LabDailyAvailabilityProvider
    {
        private static DateTime StartOfDay(DateTime date, LabConfigOpeningHours openingHoursOptions) =>
            new DateTime(date.Year, date.Month, date.Day, openingHoursOptions.StartDayHour, openingHoursOptions.StartDayMinutes, 0);

        private static DateTime EndOfDay(DateTime date, LabConfigOpeningHours openingHoursOptions) =>
            new DateTime(date.Year, date.Month, date.Day, openingHoursOptions.EndDayHour, openingHoursOptions.EndDayMinutes, 0);

        public static DateTime GetStartOfDay(DateTime date, LabConfigOpeningHours openingHoursOptions)
        {
            return StartOfDay(date, openingHoursOptions);
        }

        public static DateTime GetEndOfDay(DateTime date, LabConfigOpeningHours openingHoursOptions)
        {
            return EndOfDay(date, openingHoursOptions);
        }

        public static bool IsWhenOfficeIsOpen(DateTime date, LabConfigOpeningHours openingHoursOptions)
        {
            return IsWorkingDay(date, openingHoursOptions) && date >= StartOfDay(date, openingHoursOptions) && date < EndOfDay(date, openingHoursOptions);
        }

        public static bool IsWorkingDay(DateTime date, LabConfigOpeningHours openingHoursOptions)
        {
            return openingHoursOptions.WorkingDays.Contains(date.DayOfWeek.ToString());
        }
    }
}
