using Microsoft.EntityFrameworkCore;
using TechXpress.Web.Models;

namespace TechXpress.Web.Data
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) :base(options)
        {
            
        }
        public DbSet<Category> Categories  { get; set; }
    }
}
