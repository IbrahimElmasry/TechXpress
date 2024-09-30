using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using TechXpress.DataAccess.Data;
using TechXpress.Entities.Models;
using TechXpress.Entities.Repositories;
using TechXpress.Entities.ViewModels;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetData()
        {
            var categories = _unitOfWork.Product.GetAll(IncludeWord: "Category");
            return Json(new { data = categories });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects the HTTP post form from cross-site forgery attacks
        public IActionResult Create(ProductVM productVM, IFormFile upload)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;
                if (upload != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(upload.FileName);

                    using (var filestream = new FileStream(Path.Combine(Upload, filename + ext), FileMode.Create))
                    {
                        upload.CopyTo(filestream);
                    }
                    productVM.product.Img = @"Images\Products\" + filename + ext;
                }

                _unitOfWork.Product.Add(productVM.product);
                _unitOfWork.complete();
                TempData["Create"] = "Item has Created Successfully";
                return RedirectToAction("Index");
            }
            return View(productVM.product);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }

            ProductVM productVM = new ProductVM()
            {
                product = _unitOfWork.Product.GetFirstOrDfeault(x => x.ID == id),
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    if (productVM.product.Img != null)
                    {
                        var oldimg = Path.Combine(RootPath, productVM.product.Img.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimg))
                        {
                            System.IO.File.Delete(oldimg);
                        }
                    }

                    using (var filestream = new FileStream(Path.Combine(Upload, filename + ext), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productVM.product.Img = @"Images\Products\" + filename + ext;
                }
                _unitOfWork.Product.Update(productVM.product);
                _unitOfWork.complete();
                TempData["Update"] = "Data has Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(productVM.product);
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productIndb = _unitOfWork.Product.GetFirstOrDfeault(x => x.ID == id);
            if (productIndb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            _unitOfWork.Product.Remove(productIndb);
            var oldimg = Path.Combine(_webHostEnvironment.WebRootPath, productIndb.Img.TrimStart('\\'));
            if (System.IO.File.Exists(oldimg))
            {
                System.IO.File.Delete(oldimg);
            }
            _unitOfWork.complete();
            return Json(new { success = true, message = "file has been Deleted" });
        }
    }
}
