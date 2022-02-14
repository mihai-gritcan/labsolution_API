using LabSolution.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LabSolution.Utils
{
    public static class LabDailyAvailabilityProvider
    {
        private static DateTime StartOfDay(DateTime date, OpeningHoursDto openingHoursDto)
        {
            var d = new DateTime(date.Year, date.Month, date.Day);
            return d.Date + openingHoursDto.OpenTime;
        }

        private static DateTime EndOfDay(DateTime date, OpeningHoursDto openingHoursDto)
        {
            var d = new DateTime(date.Year, date.Month, date.Day);
            return d.Date + openingHoursDto.CloseTime;
        }

        public static bool IsWhenOfficeIsOpen(DateTime date, List<OpeningHoursDto> openingHours)
        {
            var match = openingHours.Find(x => x.DayOfWeek.Equals(date.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (match is null) return false;

            return IsWorkingDay(date, openingHours) && date >= StartOfDay(date, match) && date < EndOfDay(date, match);
        }

        public static bool IsWorkingDay(DateTime date, List<OpeningHoursDto> openingHours)
        {
            return openingHours.Select(x => x.DayOfWeek).Contains(date.DayOfWeek.ToString());
        }

        public static DateTime GetStartOfDay(DateTime date, List<OpeningHoursDto> openingHours)
        {
            var match = openingHours.Find(x => x.DayOfWeek.Equals(date.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return StartOfDay(date, match);
        }

        public static DateTime GetEndOfDay(DateTime date, List<OpeningHoursDto> openingHours)
        {
            var match = openingHours.Find(x => x.DayOfWeek.Equals(date.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return EndOfDay(date, match);
        }
    }
}
