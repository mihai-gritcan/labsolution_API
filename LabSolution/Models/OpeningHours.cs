using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(OpeningHours))]
    [Index(nameof(DayOfWeek), Name = "IX_OpeningHours_DayOfWeek", IsUnique = true)]
    public class OpeningHours
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string DayOfWeek { get; set; }
        [Required]
        public TimeSpan OpenTime { get; set; }
        [Required]
        public TimeSpan CloseTime { get; set; }

        [Column(TypeName = "date")]
        public DateTime ApplicableFrom { get; set; }
        [Column(TypeName = "date")]
        public DateTime ApplicableTo { get; set; }
    }
}