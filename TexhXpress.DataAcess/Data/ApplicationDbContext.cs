using Microsoft.EntityFrameworkCore;
using TechXpress.Entities.Models;

namespace TechXpress.DataAccess.Data
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) :base(options)
        {
            
        }
        public DbSet<Category> Categories  { get; set; }
        public DbSet<Product> Products { get; set; }

    }
}
