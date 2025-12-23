using Demo1Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Table> Tables { get; set; }
         public DbSet<TableGroup> TableGroups { get; set; }

        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceDetail>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);
        }


    }

}
