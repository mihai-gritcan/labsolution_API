using LabSolution.Dtos;
using LabSolution.Infrastructure;
using System;

namespace LabSolution.HttpModels
{
    public class CreatedOrdersResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Scheduled { get; set; }
        public DateTime Placed { get; set; }
        public TestType TestType { get; set; }
        public TestLanguage PrefferedLanguage { get; set; }
        public int? ParentId { get; set; }

        public CustomerDto Customer { get; set; }
    }
}
