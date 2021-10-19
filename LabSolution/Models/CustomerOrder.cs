using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(CustomerOrder))]
    [Index(nameof(CustomerId), Name = "IX_CustomerOrder_CustomerId")]
    public partial class CustomerOrder
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Scheduled { get; set; }
        public DateTime Placed { get; set; }
        public short TestType { get; set; }
        public short PrefferedLanguage { get; set; }
        public int? ParentId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [InverseProperty("CustomerOrders")]
        public virtual Customer Customer { get; set; }

        [InverseProperty("CustomerOrder")]
        public virtual ProcessedOrder ProcessedOrder { get; set; }
    }
}