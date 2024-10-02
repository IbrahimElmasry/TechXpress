using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Entities.Models;

namespace TechXpress.Entities.ViewModels
{
       public  class ShoppingCart

       {
        public Product Product { get; set; }


        [Range(1,100 , ErrorMessage ="You must enter a value between 1 to 100")]
        public int Count { get; set; }
       }
}
