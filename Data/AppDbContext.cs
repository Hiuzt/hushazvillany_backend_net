using hushazvillany_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace hushazvillany_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Messages> Messages { get; set; }



    }
}
