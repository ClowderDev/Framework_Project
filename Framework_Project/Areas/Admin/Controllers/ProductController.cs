using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Product")]
     [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnviroment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(c => c.Category).Include(b => b.Brand).ToListAsync());
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            // Lấy lại danh sách Category và Brand để giữ lại lựa chọn của người dùng nếu có lỗi validation
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong database");
                    return View(product);
                }

                // Xử lý upload hình ảnh nếu có
                if (product.ImageUpload != null)
                {
                    // Xác định thư mục lưu trữ hình ảnh sản phẩm
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/products");
                    // Tạo tên file hình ảnh duy nhất bằng GUID và tên file gốc
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    // Tạo đường dẫn đầy đủ đến file hình ảnh
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // Tạo FileStream để ghi file
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    // Sao chép nội dung file upload vào FileStream bất đồng bộ
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    // Lưu tên file hình ảnh vào model sản phẩm
                    product.Image = imageName;
                }
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(product);
        }

        [HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			return View(product);
		}

        [Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductModel product)
		{
			var existed_product = _dataContext.Products.Find(product.Id); 
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			if (ModelState.IsValid)
			{
				product.Slug = product.Name.Replace(" ", "-");

				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();
					existed_product.Image = imageName;
				}

				existed_product.Name = product.Name;
				existed_product.Description = product.Description;
				existed_product.Price = product.Price;
				existed_product.CategoryId = product.CategoryId;
				existed_product.BrandId = product.BrandId;

				_dataContext.Update(existed_product);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật sản phẩm thành công";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			return View(product);
		}

        [HttpDelete]
        [Route("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
            // Kiểm tra nếu hình ảnh sản phẩm không phải là "noname.jpg"
			if (!string.Equals(product.Image, "noname.jpg"))
			{
                // Xác định thư mục lưu trữ hình ảnh sản phẩm
				string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/products");
                // Tạo đường dẫn đầy đủ đến file hình ảnh cũ
				string oldfilePath = Path.Combine(uploadsDir, product.Image);
                // Kiểm tra xem file hình ảnh cũ có tồn tại không
				if (System.IO.File.Exists(oldfilePath))
				{
                    // Nếu tồn tại, xóa file hình ảnh cũ
					System.IO.File.Delete(oldfilePath);
				}
			}
			_dataContext.Products.Remove(product);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "sản phẩm đã được xóa thành công";
			return RedirectToAction("Index");
		}

        [Route("CreateProductQuantity")]
        [HttpGet]
        public async Task<IActionResult> CreateProductQuantity(long Id)
        {
            var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productbyquantity;
            ViewBag.ProductId = Id;
            return View();
        }

        [Route("UpdateMoreQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMoreQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound(); 
            }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateTime = DateTime.Now;

            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("CreateProductQuantity", "Product", new { Id = productQuantityModel.ProductId });
        }

    }
}