using Microsoft.AspNetCore.Mvc;
using TechXpress.DataAccess.Data;
using TechXpress.Entities.Models;
using TechXpress.Entities.Repositories;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // protects the http post form for cross side forgery attacks
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // _context.Categories.Add(category);
                _unitOfWork.Category.Add(category);

                //_context.SaveChanges();
                _unitOfWork.complete();

                TempData["Create"] = "Item has been created successfull";

                return RedirectToAction("Index");
            }
            return View(category);

        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            //  var category = _context.Categories.Find(id);
            var category = _unitOfWork.Category.GetFirstOrDfeault(x => x.Id == id);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                //_context.Categories.Update(category);
                //_context.SaveChanges();
                _unitOfWork.Category.Update(category);
                _unitOfWork.complete();
                TempData["Update"] = "Item has been Updated successfull";
                return RedirectToAction("Index");
            }
            return View(category);
        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            var category = _unitOfWork.Category.GetFirstOrDfeault(x => x.Id == id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int? id)
        {

            var category = _unitOfWork.Category.GetFirstOrDfeault(x => x.Id == id);
            if (category == null)
            {
                NotFound();
            }
            //_context.Categories.Remove(category);
            //_context.SaveChanges();
            _unitOfWork.Category.Remove(category);
            _unitOfWork.complete();
            TempData["Delete"] = "Item has been deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
