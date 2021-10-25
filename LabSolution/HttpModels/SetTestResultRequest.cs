using LabSolution.Infrastructure;

namespace LabSolution.HttpModels
{
    public class SetTestResultRequest
    {
        public int ProcesedOrderId { get; set; }
        public TestResult TestResult { get; set; }
        public string ExecutorName { get; set; }
        public string VerifierName { get; set; }
        public string ValidatorName { get; set; }
    }
}
