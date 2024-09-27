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
            var products = _unitOfWork.Product.GetAll(IncludeWord: "Category") // Include the Category if needed
                .Select(p => new
                {
                    p.ID,
                    p.Name,
                    p.Description,
                    p.Price,
                    CategoryName = p.Category.Name // Ensure you access the name correctly
                }).ToList();

            return Json(new { data = products });
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
                // Get the path to the wwwroot folder
                string rootPath = _webHostEnvironment.WebRootPath;

                if (upload != null && upload.Length > 0) // Check if a file was uploaded
                {
                    // Generate a new filename using GUID to avoid conflicts
                    string filename = Guid.NewGuid().ToString();
                    var uploadPath = Path.Combine(rootPath, "@Images", "Products");
                    var ext = Path.GetExtension(upload.FileName);

                    // Ensure the upload directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Combine the upload path with the new filename and extension
                    string fullPath = Path.Combine(uploadPath, filename + ext);

                    // Use a using statement to ensure the file stream is properly disposed
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        // Copy the uploaded file to the new file stream
                        upload.CopyTo(fileStream);
                    }

                    // Set the image path in the product view model
                    productVM.product.Img = Path.Combine("Images", "Products", filename + ext).Replace("\\", "/"); // Ensure the path is correct for web serving
                }

                // Add the product to the database using Unit of Work
                _unitOfWork.Product.Add(productVM.product);
                _unitOfWork.complete();

                TempData["Create"] = "Item has been created successfully";

                return RedirectToAction("Index");
            }
            return View(productVM.product);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            //  var product = _context.Products.Find(id);
            var product = _unitOfWork.Product.GetFirstOrDfeault(x => x.ID == id);
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                //_context.Products.Update(product);
                //_context.SaveChanges();
                _unitOfWork.Product.Update(product);
                _unitOfWork.complete();
                TempData["Update"] = "Item has been deleted successfull";
                return RedirectToAction("Index");
            }
            return View(product);
        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            var product = _unitOfWork.Product.GetFirstOrDfeault(x => x.ID == id);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int? id)
        {

            var product = _unitOfWork.Product.GetFirstOrDfeault(x => x.ID == id);
            if (product == null)
            {
                NotFound();
            }
            //_context.Products.Remove(product);
            //_context.SaveChanges();
            _unitOfWork.Product.Remove(product);
            _unitOfWork.complete();
            TempData["Delete"] = "Item has been deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
