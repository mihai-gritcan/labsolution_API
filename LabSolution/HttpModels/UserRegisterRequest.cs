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
        /// At least one lower case letter,
        /// At least one upper case letter,
        /// At least special character,
        /// At least one number
        /// At least 8 characters length
        /// </summary>
        /// <see cref="https://www.c-sharpcorner.com/uploadfile/jitendra1987/password-validator-in-C-Sharp/"/>
        [Required]
        [RegularExpression(@"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$")]
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
