using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Entities.Repositories;
using TechXpress.Utilities;

namespace TechXpress.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class DashboardController : Controller
	{

		private IUnitOfWork _unitOfWork;
		public DashboardController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			ViewBag.Orders = _unitOfWork.OrderHeader.GetAll().Count();
			ViewBag.ApprovedOrders = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == SD.Approve).Count();
			ViewBag.Users = _unitOfWork.ApplicationUser.GetAll().Count();
			ViewBag.Products = _unitOfWork.Product.GetAll().Count();
			return View();
		}
	}
}
