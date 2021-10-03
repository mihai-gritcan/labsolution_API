using LabSolution.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LabSolution.HttpModels
{
    public class CreateOrderRequest : IValidatableObject
    {
        [Required]
        public DateTime ScheduledTime { get; set; }

        public List<CustomerDto> Customers { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationErrors = new List<ValidationResult>();

            if (Customers?.Count == 0)
                validationErrors.Add(new ValidationResult($"{nameof(Customers)} cannot be null or empty", new List<string> { nameof(Customers) }));

            if(Customers.Count > 1 && Customers.Count(x => x.IsRootCustomer) != 1)
                validationErrors.Add(new ValidationResult("Please set one single customer as Root customer", new List<string> { nameof(Customers) }));

            return validationErrors;
        }
    }
}
