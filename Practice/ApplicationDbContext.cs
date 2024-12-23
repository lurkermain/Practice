using Microsoft.EntityFrameworkCore;
using Practice.Models;

namespace Practice
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        {
            Database.EnsureCreated();
        }

    }
}
