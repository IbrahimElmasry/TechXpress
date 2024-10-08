using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Entities.Models;

namespace TechXpress.Entities.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable <ShoppingCart> CartsList { get; set; }
        public decimal TotalCarts { get; set; }
    }
}
