using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TechXpress.Entities.Repositories;

namespace TechXpress.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IUnitOfWork _unitOfWork;
        public BaseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public override  void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Fetch categories from the database
            var categories = _unitOfWork.Category.GetAll().ToList();

            // Make the categories available in the ViewBag
            ViewBag.Categories = categories;

            base.OnActionExecuting(filterContext);
        }
    }
}
