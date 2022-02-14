using LabSolution.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LabSolution.Utils
{
    public static class LabDailyAvailabilityProvider
    {
        public static bool IsWhenOfficeIsOpen2(DateTime date, List<OpeningHoursDto> openingHours)
        {
            var match = openingHours.Find(x => x.DayOfWeek.Equals(date.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (match is null) return false;

            return IsWorkingDay2(date, openingHours) && date >= StartOfDay2(date, match) && date < EndOfDay2(date, match);
        }

        public static bool IsWorkingDay2(DateTime date, List<OpeningHoursDto> openingHours)
        {
            return openingHours.Select(x => x.DayOfWeek).Contains(date.DayOfWeek.ToString());
        }

        private static DateTime StartOfDay2(DateTime date, OpeningHoursDto openingHoursDto)
        {
            var d = new DateTime(date.Year, date.Month, date.Day);
            var x = d.Date + openingHoursDto.OpenTime;
            return d.Date + openingHoursDto.OpenTime;
        }

        private static DateTime EndOfDay2(DateTime date, OpeningHoursDto openingHoursDto)
        {
            var d = new DateTime(date.Year, date.Month, date.Day);
            var y = d.Date + openingHoursDto.CloseTime;
            return d.Date + openingHoursDto.CloseTime;
        }

        public static DateTime GetStartOfDay2(DateTime date, List<OpeningHoursDto> openingHours)
        {
            var match = openingHours.Find(x => x.DayOfWeek.Equals(date.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));

            return StartOfDay2(date, match);
        }

        public static DateTime GetEndOfDay2(DateTime date, List<OpeningHoursDto> openingHours)
        {
            var match = openingHours.Find(x => x.DayOfWeek.Equals(date.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return EndOfDay2(date, match);
        }

    }
}
