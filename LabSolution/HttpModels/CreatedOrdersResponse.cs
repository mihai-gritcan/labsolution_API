using LabSolution.Dtos;
using LabSolution.Enums;
using System;

namespace LabSolution.HttpModels
{
    public class BaseOrder {
        public CustomerDto Customer { get; set; }
        public TestType TestType { get; set; }
        public TestLanguage TestLanguage { get; set; }
    }

    public class CreatedOrdersResponse : BaseOrder
    {
        public int OrderId { get; set; }

        public DateTime Scheduled { get; set; }
        public DateTime PlacedAt { get; set; }
        public int CustomerId { get; set; }
        public int? ParentId { get; set; }
    }

    public class ProcessedOrderResponse : BaseOrder
    {
        public int ProcessedOrderId { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string NumericCode { get; set; }
        public string Barcode { get; set; }
    }

    public class ProcessedOrderForPdf : BaseOrder
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }
        public TestResult TestResult { get; set; }
        public string NumericCode { get; set; }
        public DateTime ProcessedAt { get; internal set; }
        public string ProcessedBy { get; internal set; }
        public string PdfName { get; internal set; }
    }

    public class OrderWithStatusResponse : BaseOrder
    {
        public int OrderId { get; set; }
        public string NumericCode { get; set; }
        public int? ParentId { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }
        public TestResult? TestResult { get; set; }
        public int? ProcessedOrderId { get; internal set; }
    }

    public class ProcessedOrderToSetResultResponse
    {
        public int ProcessedOrderId { get; set; }
        public string NumericCode => ProcessedOrderId.ToString("D7");
        public DateTime ProcessedAt { get; set; }
        public string PersonalNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public TestResult? TestResult { get; set; }
    }

    public enum OrderStatus
    {
        Created = 1,
        Processed = 2
    }
}
