using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.Entities.Models;
using TechXpress.Entities.Repositories;
using TechXpress.Entities.ViewModels;
using TechXpress.Utilities;

namespace TechXpress.Web.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int TotalCarts { get; set; }

        public CartController (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
    }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value , IncludeWord:"Product")
            };

            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.TotalCarts += (item.Count * item.Product.Price);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus (int cartid) 
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x=>x.Id==cartid);
            _unitOfWork.ShoppingCart.IncreaseCount(shoppingcart, 1);
            _unitOfWork.complete();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x => x.Id == cartid);

            if (shoppingcart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingcart);
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingcart.ApplicationUserId).ToList().Count() - 1;
                return RedirectToAction("Index" , "Home");
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingcart, 1);

            }
            _unitOfWork.complete();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x => x.Id == cartid);
            _unitOfWork.ShoppingCart.Remove(shoppingcart);
            _unitOfWork.complete();
            return RedirectToAction("Index");
        }
    }
}
