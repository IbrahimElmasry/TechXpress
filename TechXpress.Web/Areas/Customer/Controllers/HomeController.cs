using Microsoft.AspNetCore.Mvc;
using TechXpress.Entities.Models;
using TechXpress.Entities.Repositories;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }

        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll();

            return View(products);
        }
        public IActionResult Details(int Id) 
        { 
            ShoppingCart obj = new ShoppingCart()
            {
                Product = _unitOfWork.Product.GetFirstOrDfeault(x => x.ID == Id, IncludeWord: "Category"),
                Count = 1 
            };
            return View(obj);
        }
    }
}
