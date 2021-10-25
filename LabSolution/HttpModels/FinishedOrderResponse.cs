using LabSolution.Dtos;
using LabSolution.Infrastructure;
using System;

namespace LabSolution.HttpModels
{
    public class FinishedOrderResponse
    {
        public int Id { get; set; }
        public TestResult TestResult { get; set; }
        public string NumericCode => Id.ToString("D7");
        public DateTime OrderDate { get; set; }
        public CustomerDto Customer { get; set; }
        public TestLanguage TestLanguage { get; set; }
        public TestType TestType { get; set; }
    }
}
