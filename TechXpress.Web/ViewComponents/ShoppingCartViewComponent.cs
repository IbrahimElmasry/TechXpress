using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.Entities.Repositories;
using TechXpress.Utilities;

namespace TechXpress.Web.ViewComponents
{
    // The ShoppingCartViewComponent class is responsible for displaying the shopping cart item count in the UI
    public class ShoppingCartViewComponent : ViewComponent
    {
        // Using the Unit of Work pattern to handle data access
        private readonly IUnitOfWork _unitofwork;

        // Constructor to inject the Unit of Work dependency
        public ShoppingCartViewComponent(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        // The InvokeAsync method is called to execute the view component and return the result
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Retrieve the claims identity of the currently logged-in user
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            // Find the user's unique identifier (NameIdentifier claim)
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // If the claim is not null, the user is logged in
            if (claim != null)
            {
                // Check if the shopping cart item count is already stored in the session
                if (HttpContext.Session.GetInt32(SD.SessionKey) != null)
                {
                    // If the session already contains the item count, return it to the view
                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
                else
                {
                    // If not, retrieve the number of items in the shopping cart from the database
                    // and store it in the session for future use
                    HttpContext.Session.SetInt32(SD.SessionKey, _unitofwork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count());

                    // Return the item count to the view
                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
            }
            else
            {
                // If the claim is null (user is not logged in), clear the session and return a count of 0
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
