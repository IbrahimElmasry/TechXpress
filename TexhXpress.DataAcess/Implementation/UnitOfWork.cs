using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.DataAccess.Data;
using TechXpress.Entities.Repositories;

namespace TexhXpress.DataAccess.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category=new CategoryRepository(context);

            Product=new ProductRepository(context);

            ShoppingCart=new ShoppingCartRepository(context);
    }
            public ICategoryRepository Category { get; private set; }

            public IProductRepository Product { get; private set; }

            public IShoppingCartRepository ShoppingCart { get; private set; }



        public int complete()
        {
           return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
