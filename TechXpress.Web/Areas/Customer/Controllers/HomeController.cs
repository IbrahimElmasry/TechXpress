using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.DataAccess.Data;
using TechXpress.Entities.Models;
using TechXpress.Entities.Repositories;
using TechXpress.Utilities;
using TechXpress.Web.Controllers;
using TexhXpress.DataAccess.Implementation;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
       

        public HomeController(IUnitOfWork unitOfWork):base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }

        public IActionResult Index(int categoryId)
        {
            //var products = _unitOfWork.Product.GetAll();

            //return View(products);

            if (categoryId == null || categoryId==0)
            {
                // If no category is selected, return all products
                var allProducts = _unitOfWork.Product.GetAll().ToList();
                return View(allProducts);
            }

            // Fetch products by the selected category
            var products = _unitOfWork.Product
                          .GetAll(p => p.CategoryId == categoryId)
                          .ToList();

            return View(products);
        }
      
        public IActionResult Details(int ProductId)
        {
            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = ProductId,
                Product = _unitOfWork.Product.GetFirstOrDfeault(v => v.ID == ProductId, IncludeWord: "Category"),
                Count = 1
            };
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart Cartobj = _unitOfWork.ShoppingCart.GetFirstOrDfeault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if (Cartobj == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.complete();


            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(Cartobj, shoppingCart.Count);
                _unitOfWork.complete();
            }


            return RedirectToAction("Index");
        }
    }
}
