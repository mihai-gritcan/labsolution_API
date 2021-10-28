using System.ComponentModel.DataAnnotations;

namespace LabSolution.HttpModels
{
    public class UserRegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        [RegularExpression("^(?=.*?[A - Z])(?=.*?[a - z])(?=.*?[0 - 9])(?=.*?[#?!@$%^&*-]).{8,}$)")]
        public string Password { get; set; }
        [Required]
        [StringLength(50)]
        public string Firstname { get; set; }
        [Required]
        [StringLength(50)]
        public string Lastname { get; set; }
    }
}
