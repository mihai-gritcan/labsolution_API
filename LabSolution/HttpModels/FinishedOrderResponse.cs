using LabSolution.Infrastructure;
using System;

namespace LabSolution.HttpModels
{
    public class FinishedOrderResponse
    {
        public int Id { get; set; }
        public TestResult TestResult { get; set; }
        public long NumericCode { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
