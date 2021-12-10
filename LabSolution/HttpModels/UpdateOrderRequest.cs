using LabSolution.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LabSolution.HttpModels
{
    public class UpdateOrderRequest : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

        public string ScheduledDate { get; set; }

        public string ScheduledTime { get; set; }

        public int TestType { get; set; } = (int)Enums.TestType.Antigen;

        public int TestLanguage { get; set; } = (int)Enums.TestLanguage.Romanian;

        [JsonIgnore]
        public DateTime ScheduledDateTime
        {
            get
            {
                var dateTimeString = $"{ScheduledDate} {ScheduledTime}";
                DateTime.TryParse(dateTimeString, out var parsedDate);
                return parsedDate;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationErrors = new List<ValidationResult>();

            var dateTimeString = $"{ScheduledDate} {ScheduledTime}";

            if (!DateTime.TryParse(dateTimeString, out var parsedDate))
                validationErrors.Add(new ValidationResult($"Invalid Date or Time Format '{dateTimeString}'", new List<string> { nameof(ScheduledDate) }));

            return validationErrors;
        }
    }
}
