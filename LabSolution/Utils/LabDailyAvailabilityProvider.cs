using System;

namespace LabSolution.Utils
{
    public static class LabDailyAvailabilityProvider
    {
        // Default configuration is from 8:00 to 18:00 -> 5 places at each 10 minutes

        private const int _defaultStartHour = 8;
        private const int _defaultEndHour = 18;

        public const int DefaultPlacesPer10Minutes = 5;

        public static DateTime StartOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day, _defaultStartHour, 0, 0);
        public static DateTime EndOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day, _defaultEndHour, 0, 0);

        public static bool IsWhenOfficeIsOpen(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday && date >= StartOfDay(date) && date < EndOfDay(date);
        }
    }
}
