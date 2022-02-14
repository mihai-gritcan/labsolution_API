using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(DaysOff))]
    public class DaysOff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public bool IsLegalHoliday { get; set; } // true - for legal holiday, false - for custom days off

        public bool IsHolidayDeactivated { get; set; } // true - when the legal holiday will be a normal working day

        public int ApplicableYear { get; set; } // 0 - for permanent holiday
    }
}