using System;
using System.Collections.Generic;

namespace LabSolution.Dtos
{
    public class PriceStatisticsDto
    {
        public DateTime Date { get; set; }
        public List<TestStats> TestStats { get; set; }
    }

    public class TestStats
    {
        public short Type { get; set; }
        public int DailyCount { get; set; }
        public decimal DailyAmount { get; set; }
    }
}