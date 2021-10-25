using LabSolution.Dtos;
using LabSolution.Infrastructure;
using System;

namespace LabSolution.HttpModels
{
    public class ProcessedOrderResponse
    {
        public int Id { get; set; }
        public int CustomerOrderId { get; set; }
        public string NumericCode { get; set; }
        public byte[] Barcode { get; set; }
        public DateTime ProcessedAt { get; set; }
        public TestType TestType { get; set; }
        public TestLanguage TestLanguage { get; set; }
        public CustomerDto Customer { get; internal set; }
    }
}
