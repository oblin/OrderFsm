using Microsoft.EntityFrameworkCore;

namespace OrderFsm.Models
{
    public class OrderContext : DbContext
    {
        public OrderContext() { }
        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }
        public DbSet<OrderProcess> OrderProcesses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=Msmtest;Trusted_Connection=True;");
        }
    }
}
