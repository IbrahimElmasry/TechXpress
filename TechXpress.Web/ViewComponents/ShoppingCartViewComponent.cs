using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.Entities.Repositories;
using TechXpress.Utilities;

namespace TechXpress.Web.ViewComponents
{
    // The ShoppingCartViewComponent class is responsible for displaying the shopping cart item count in the UI
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitofwork;
        public ShoppingCartViewComponent(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                // Clear the session if the user has changed
                if (HttpContext.Session.GetString("UserId") != claim.Value)
                {
                    HttpContext.Session.Clear(); // Clear the previous session data
                    HttpContext.Session.SetString("UserId", claim.Value); // Set the new user ID in session
                }

                // Check if the session already has the cart count
                if (HttpContext.Session.GetInt32(SD.SessionKey) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
                else
                {
                    // Retrieve the cart count from the database for the current user
                    var cartCount = _unitofwork.ShoppingCart
                        .GetAll(x => x.ApplicationUserId == claim.Value)
                        .ToList()
                        .Count();

                    HttpContext.Session.SetInt32(SD.SessionKey, cartCount);
                    return View(cartCount);
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }

    }
}
