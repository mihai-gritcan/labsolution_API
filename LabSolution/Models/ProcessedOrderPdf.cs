using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(ProcessedOrderPdf))]
    [Index(nameof(ProcessedOrderId), Name = "IX_ProcessedOrderPdf_ProcessedOrderId", IsUnique = true)]
    public class ProcessedOrderPdf
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        public byte[] PdfBytes { get; set; }

        public int ProcessedOrderId { get; set; }
        [ForeignKey(nameof(ProcessedOrderId))]
        [InverseProperty("ProcessedOrderPdf")]
        public virtual ProcessedOrder ProcessedOrder { get; set; }
    }
}
