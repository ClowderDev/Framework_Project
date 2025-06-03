using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy tất cả Brand từ database, sắp xếp giảm dần theo Id và chuyển thành List bất đồng bộ
            // Trả về View với danh sách Brand
            return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                // Tạo slug từ tên thương hiệu bằng cách thay thế khoảng trắng bằng dấu gạch ngang
                brand.Slug = brand.Name.Replace(" ", "-");

                // Kiểm tra xem slug đã tồn tại trong database chưa
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã có trong database");
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thương hiệu thành công";
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
            return View(brand);
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thương hiệu thành công";
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
            return View(brand);
        }

        [HttpDelete]
        [Route("Delete/{id:long}")]
        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thương hiệu đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
