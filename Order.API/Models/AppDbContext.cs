using Microsoft.EntityFrameworkCore;

namespace Order.API.Models
{
    public class AppDbContext:DbContext
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

    }
}
