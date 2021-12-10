using System.ComponentModel.DataAnnotations;

namespace LabSolution.Dtos
{
    public class AppConfigDto
    {
        public int Id { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }
    }
}

