using LabSolution.Dtos;
using LabSolution.Infrastructure;
using System;

namespace LabSolution.HttpModels
{
    public class BaseOrder {
        public int OrderId { get; set; }
        public CustomerDto Customer { get; set; }
        public TestType TestType { get; set; }
        public TestLanguage TestLanguage { get; set; }
    }

    public class CreatedOrdersResponse : BaseOrder
    {
        public DateTime Scheduled { get; set; }
        public DateTime PlacedAt { get; set; }
        public int CustomerId { get; set; }
        public int? ParentId { get; set; }
    }

    public class ProcessedOrderResponse : BaseOrder
    {
        public DateTime ProcessedAt { get; set; }
        public string NumericCode { get; set; }
        public string Barcode { get; set; }
    }

    public class ProcessedOrderForPdf : BaseOrder
    {
        public DateTime OrderDate { get; set; }
        public TestResult TestResult { get; set; }
        public string NumericCode { get; set; }
        public DateTime ProcessedAt { get; internal set; }
        public string ProcessedBy { get; internal set; }
    }

    public class OrderWithStatusResponse : BaseOrder
    {
        public string NumericCode { get; set; }
        public int? ParentId { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }
        public TestResult? TestResult { get; set; }
        public object ProcessedOrderId { get; internal set; }
    }

    public enum OrderStatus
    {
        Created = 1,
        Processed = 2
    }
}
