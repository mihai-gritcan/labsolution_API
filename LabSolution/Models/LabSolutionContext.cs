using Microsoft.EntityFrameworkCore;

namespace LabSolution.Models
{
    public partial class LabSolutionContext : DbContext
    {
        public LabSolutionContext()
        {
        }

        public LabSolutionContext(DbContextOptions<LabSolutionContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerOrder> CustomerOrders { get; set; }
        public virtual DbSet<ProcessedOrder> ProcessedOrders { get; set; }
        public virtual DbSet<AppConfig> AppConfigs { get; set; }
        public virtual DbSet<ProcessedOrderPdf> ProcessedOrderPdfs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerOrders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerOrder_Customer");
            });

            modelBuilder.Entity<ProcessedOrder>(entity =>
            {
                entity.HasOne(d => d.CustomerOrder)
                    .WithOne(p => p.ProcessedOrder)
                    .HasForeignKey<ProcessedOrder>(d => d.CustomerOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProcessedOrder_CustomerOrder");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}