using LabSolution.Infrastructure;

namespace LabSolution.HttpModels
{
    public class SetTestResultRequest
    {
        public int OrderId { get; set; }
        public long NumericCode { get; set; }
        public TestResult TestResult { get; set; }
    }
}
