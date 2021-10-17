using System.ComponentModel.DataAnnotations;

namespace LabSolution.HttpModels
{
    public class UserRegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
