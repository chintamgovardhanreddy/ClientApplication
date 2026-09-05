using ClientApplication.model.Entity;
using Microsoft.EntityFrameworkCore;

namespace ClientApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }
        public DbSet<Client> clients { get; set; }
        // Add this line
        public DbSet<User> Users { get; set; }
    }
}
