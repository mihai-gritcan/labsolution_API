using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabSolution.HttpModels
{
    public class UserRegisterRequest : IValidatableObject
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// At least one upper case English letter, (?=.*?[A-Z])
        /// At least one lower case English letter, (?=.*?[a - z])
        /// At least one digit, (?=.*?[0 - 9])
        /// At least one special character, (?=.*?[#?!@$%^&*-])
        /// Minimum eight in length.{8,}(with the anchors)
        /// </summary>
        /// <see cref="https://stackoverflow.com/questions/19605150/regex-for-password-must-contain-at-least-eight-characters-at-least-one-number-a"/>
        [Required]
        [RegularExpression("^(?=.*?[A - Z])(?=.*?[a - z])(?=.*?[0 - 9])(?=.*?[#?!@$%^&*-]).{8,}$)")]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string Firstname { get; set; }
        [Required]
        [StringLength(50)]
        public string Lastname { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationErrors = new List<ValidationResult>();

            if (!string.Equals(Password, ConfirmPassword))
                validationErrors.Add(new ValidationResult($"{nameof(Password)} and {nameof(ConfirmPassword)} fields should equal", new List<string> { nameof(Password), nameof(ConfirmPassword) }));

            return validationErrors;
        }
    }
}
