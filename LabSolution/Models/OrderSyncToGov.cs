using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(OrderSyncToGov))]
    [Index(nameof(ProcessedOrderId), Name = "IX_OrderSyncToGov_ProcessedOrderId", IsUnique = true)]
    public class OrderSyncToGov
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateSynched { get; set; }

        public bool? TestResultSyncStatus { get; set; }

        public int ProcessedOrderId { get; set; }
        [ForeignKey(nameof(ProcessedOrderId))]
        [InverseProperty("OrderSyncToGov")]
        public virtual ProcessedOrder ProcessedOrder { get; set; }
    }
}
