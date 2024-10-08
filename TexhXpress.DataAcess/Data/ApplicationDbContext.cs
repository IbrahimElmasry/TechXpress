using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechXpress.Entities.Models;

namespace TechXpress.DataAccess.Data
{
    public class ApplicationDbContext :IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) :base(options)
        {
            
        }
        public DbSet<Category> Categories  { get; set; }


        public DbSet<Product> Products { get; set; }



        public DbSet <ApplicationUser> ApplicationUsers { get; set; }



        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<OrderDetail>orderDetails { get; set; }
        public DbSet<OrderHeader> orderHeaders { get; set; }


    }
}
