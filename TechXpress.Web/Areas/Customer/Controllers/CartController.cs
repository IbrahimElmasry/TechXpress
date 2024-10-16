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
    [Authorize] // Ensures that the user must be logged in to access the CartController
    public class CartController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; } // ViewModel to hold cart data
        public int TotalCarts { get; set; } // Tracks total price of items in the cart

        // Constructor that injects the Unit of Work dependency and calls the base controller
        public CartController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Displays the cart index view with the list of items and total price
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity; // Retrieve current user identity
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // Find user ID claim

            ShoppingCartVM = new ShoppingCartVM()
            {
                // Retrieve all shopping cart items for the logged-in user
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product")
            };

            // Calculate the total price for all items in the cart
            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.TotalCarts += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM); // Return the view with ShoppingCartVM data
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // Find user ID

            ShoppingCartVM = new ShoppingCartVM()
            {
                // Get all cart items along with their product information
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
                OrderHeader = new() // Initialize OrderHeader object
            };

            // Retrieve and populate the ApplicationUser details in the order
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDfeault(x => x.Id == claim.Value);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            // Calculate the total price of the order
            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM); // Return the summary view
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken] // CSRF protection
        public IActionResult POSTSummary(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // Retrieve all shopping cart items for the logged-in user
            ShoppingCartVM.CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product");

            // Set order status, payment status, and date
            ShoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            // Calculate total price of the order
            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            // Save the OrderHeader to the database
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.complete();

            // Save the order details for each product in the cart
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

            // Stripe payment setup
            var domain = "https://localhost:7280/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            // Add each cart item to the Stripe session
            foreach (var item in ShoppingCartVM.CartsList)
            {
                var sessionlineoption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100), // Stripe uses cents
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

            // Create the Stripe session
            var service = new SessionService();
            Session session = service.Create(options);
            ShoppingCartVM.OrderHeader.SessionId = session.Id;
            ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;

            _unitOfWork.complete();

            // Redirect to the Stripe checkout page
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        // Confirms the order and updates status upon successful payment
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDfeault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            // Update order status if payment is successful
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.complete();
            }

            // Remove all items from the user's shopping cart
            List<ShoppingCart> shoppingcarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            HttpContext.Session.Clear();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingcarts);
            _unitOfWork.complete();
            return View(id);
        }

        // Increases the quantity of a cart item
        public IActionResult Plus(int cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x => x.Id == cartid);
            _unitOfWork.ShoppingCart.IncreaseCount(shoppingcart, 1);
            _unitOfWork.complete();
            return RedirectToAction("Index");
        }

        // Decreases the quantity of a cart item or removes it if the count goes below 1
        public IActionResult Minus(int cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x => x.Id == cartid);

            if (shoppingcart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingcart);
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingcart.ApplicationUserId).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingcart, 1);
            }

            _unitOfWork.complete();
            return RedirectToAction("Index");
        }

        // Removes a cart item completely
        public IActionResult Remove(int cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDfeault(x => x.Id == cartid);
            _unitOfWork.ShoppingCart.Remove(shoppingcart);
            _unitOfWork.complete();

            var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingcart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return RedirectToAction("Index");
        }
    }
}
