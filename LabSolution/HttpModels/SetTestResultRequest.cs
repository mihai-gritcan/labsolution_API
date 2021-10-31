﻿using LabSolution.Infrastructure;

namespace LabSolution.HttpModels
{
    public class SetTestResultRequest
    {
        public int ProcessedOrderId { get; set; }
        public TestResult TestResult { get; set; }
        public string ExecutorName { get; set; }
        public string VerifierName { get; set; }
        public string ValidatorName { get; set; }
    }
}
