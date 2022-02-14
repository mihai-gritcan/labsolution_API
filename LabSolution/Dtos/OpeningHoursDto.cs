using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace LabSolution.Dtos
{
    public class OpeningHoursDto : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string DayOfWeek { get; set; }
        [Required]
        public TimeSpan OpenTime { get; set; }
        [Required]
        public TimeSpan CloseTime { get; set; }

        public DateTime? ApplicableFrom { get; set; }
        public DateTime? ApplicableTo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationErrors = new List<ValidationResult>();

            var validDaysOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

            if(!validDaysOfWeek.Contains(DayOfWeek))
                validationErrors.Add(new ValidationResult($"Invalid Day name '{DayOfWeek}'", new List<string> { nameof(DayOfWeek) }));

            return validationErrors;
        }
    }
}
