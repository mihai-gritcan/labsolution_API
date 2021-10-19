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
        public bool? Result { get; set; }

        public byte[] Barcode { get; set; }

        public byte[] QRCode { get; set; }

        [ForeignKey(nameof(CustomerOrderId))]
        [InverseProperty("ProcessedOrder")]
        public virtual CustomerOrder CustomerOrder { get; set; }
    }
}
