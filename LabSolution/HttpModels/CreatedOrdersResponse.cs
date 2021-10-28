using LabSolution.Dtos;
using LabSolution.Infrastructure;
using System;

namespace LabSolution.HttpModels
{
    public class CreatedOrdersResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public CustomerDto Customer { get; set; }

        public DateTime Scheduled { get; set; }
        public DateTime PlacedAt { get; set; }
        public TestType TestType { get; set; }
        public TestLanguage TestLanguage { get; set; }
        public int? ParentId { get; set; }
    }

    public class FinishedOrderResponse
    {
        public int Id { get; set; }
        public TestResult TestResult { get; set; }
        public string NumericCode { get; set; }
        public DateTime OrderDate { get; set; }
        public CustomerDto Customer { get; set; }
        public TestLanguage TestLanguage { get; set; }
        public TestType TestType { get; set; }
    }

    public class OrderWithStatusResponse
    {
        public int Id { get; set; }
        public string NumericCode { get; set; }

        public CustomerDto Customer { get; set; }
        public TestLanguage TestLanguage { get; set; }
        public TestType TestType { get; set; }
        public int? ParentId { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }
        public TestResult? TestResult { get; set; }
    }

    public enum OrderStatus
    {
        Created = 1,
        Processed = 2
    }
}
