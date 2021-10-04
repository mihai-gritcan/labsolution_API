using System;
using System.ComponentModel.DataAnnotations;

namespace LabSolution.HttpModels
{
    public class NumericCodeRequest
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int CreatedOrderId { get; set; }
    }
}
