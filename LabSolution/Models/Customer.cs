using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabSolution.Models
{
    [Table(nameof(Customer))]
    public partial class Customer
    {
        public Customer()
        {
            CustomerOrders = new HashSet<CustomerOrder>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        public int Gender { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }
        [StringLength(250)]
        public string Address { get; set; }
        [StringLength(50)]
        public string Passport { get; set; }
        [StringLength(13)]
        public string PersonalNumber { get; set; }
        [StringLength(50)]
        public string Phone { get; set; }
        [StringLength(80)]
        public string Email { get; set; }

        [InverseProperty(nameof(CustomerOrder.Customer))]
        public virtual ICollection<CustomerOrder> CustomerOrders { get; set; }
    }
}