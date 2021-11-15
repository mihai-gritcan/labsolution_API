using LabSolution.Dtos;
using LabSolution.Infrastructure;
using LabSolution.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace LabSolution.HttpModels
{
    public class CreateOrderRequest : IValidatableObject
    {
        [Required]
        public string ScheduledDate { get; set; }

        [Required]
        public string ScheduledTime { get; set; }

        [Required]
        public TestLanguage TestLanguage { get; set; }

        [Required]
        public TestType TestType { get; set; }

        public List<CustomerDto> Customers { get; set; }

        [JsonIgnore]
        public DateTime ScheduledDateTime
        {
            get
            {
                var dateTimeString = $"{ScheduledDate} {ScheduledTime}";
                DateTime.TryParse(dateTimeString, out var parsedDate);
                DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);
                return parsedDate;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationErrors = new List<ValidationResult>();

            var dateTimeString = $"{ScheduledDate} {ScheduledTime}";

            if (!DateTime.TryParse(dateTimeString, out var parsedDate))
                validationErrors.Add(new ValidationResult($"Invalid Date or Time Format '{dateTimeString}'", new List<string> { nameof(ScheduledDate) }));

            if(!LabDailyAvailabilityProvider.IsWhenOfficeIsOpen(parsedDate))
                validationErrors.Add(new ValidationResult($"The Lab is Closed on '{dateTimeString}'", new List<string> { nameof(ScheduledDate) }));

            if (Customers?.Count == 0)
                validationErrors.Add(new ValidationResult($"{nameof(Customers)} cannot be null or empty", new List<string> { nameof(Customers) }));

            if(Customers.Count > 1 && Customers.Count(x => x.IsRootCustomer) != 1)
                validationErrors.Add(new ValidationResult("Please set one single customer as Root customer", new List<string> { nameof(Customers) }));

            if (Customers.GroupBy(x => x.PersonalNumber).Count() != Customers.Count)
                validationErrors.Add(new ValidationResult("There are customers with duplicated Personal numbers. Ensure ach Customer has it's own personal number set.", new List<string> { nameof(Customers) }));

            return validationErrors;
        }
    }
}
