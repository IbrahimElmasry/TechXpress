using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using TechXpress.Entities.Models;
using TechXpress.Entities.Repositories;
using TechXpress.Entities.ViewModels;
using TechXpress.Utilities;
using TechXpress.Web.Controllers;

namespace TechXpress.Web.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class CartController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int TotalCarts { get; set; }

        public CartController (IUnitOfWork unitOfWork) :base(unitOfWork)
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

		[HttpGet]
		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDfeault(x => x.Id == claim.Value);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

			foreach (var item in ShoppingCartVM.CartsList)
			{
				ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
			}

			return View(ShoppingCartVM);
		}

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult POSTSummary(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product");


            ShoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;


            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.complete();

            foreach (var item in ShoppingCartVM.CartsList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.complete();
            }

            var domain = "https://localhost:7280/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            foreach (var item in ShoppingCartVM.CartsList)
            {
                var sessionlineoption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionlineoption);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            ShoppingCartVM.OrderHeader.SessionId = session.Id;
            ShoppingCartVM.OrderHeader.PaymentIntentId=session.PaymentIntentId;

            _unitOfWork.complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.CartsList);
            //         _unitOfWork.Complete();
            //         return RedirectToAction("Index","Home");

        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDfeault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.complete();
            }

            List<ShoppingCart> shoppingcarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
           // HttpContext.Session.Clear();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingcarts);
            _unitOfWork.complete();
            return View(id);
        }


        public IActionResult Plus(int cartid)
		{
			var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x => x.Id == cartid);
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
			var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingcart.ApplicationUserId).ToList().Count();
			return RedirectToAction("Index");
		}
	}
}
