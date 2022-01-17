using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(ProcessedOrder))]
    [Index(nameof(CustomerOrderId), Name = "IX_ProcessedOrder_CustomerOrderId", IsUnique = true)]
    public partial class ProcessedOrder
    {
        [Key]
        public int Id { get; set; }
        public int CustomerOrderId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ProcessedAt { get; set; }
        public int? TestResult { get; set; }

        [ForeignKey(nameof(CustomerOrderId))]
        [InverseProperty("ProcessedOrder")]
        public virtual CustomerOrder CustomerOrder { get; set; }
        public int PrintCount { get; set; }
        public string ProcessedBy { get; internal set; }
        public string CheckedBy { get; internal set; }
        public string ValidatedBy { get; internal set; }
        public string PdfName { get; internal set; }

        [InverseProperty("ProcessedOrder")]
        public virtual ProcessedOrderPdf ProcessedOrderPdf { get; set; }

        [InverseProperty("ProcessedOrder")]
        public virtual OrderSyncToGov OrderSyncToGov { get; set; }
    }
}
