using System.ComponentModel.DataAnnotations;

namespace LabSolution.HttpModels
{
    public class UserLoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
