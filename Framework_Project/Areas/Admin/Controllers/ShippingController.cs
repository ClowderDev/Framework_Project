using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = "Admin")]
    public class ShippingController : Controller
    {
        private readonly DataContext _dataContext;
        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var shippingList = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shippings = shippingList;
            return View();
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingModel shippingModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingShipping = await _dataContext.Shippings
                        .AnyAsync(x => x.City == shippingModel.City && 
                                      x.District == shippingModel.District && 
                                      x.Ward == shippingModel.Ward);

                    if (existingShipping)
                    {
                        ModelState.AddModelError("", "Đã tồn tại địa chỉ này.");
                        return View(shippingModel);
                    }

                    _dataContext.Shippings.Add(shippingModel);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Thêm địa chỉ thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm địa chỉ.");
                    return View(shippingModel);
                }
            }
            return View(shippingModel);
        }
    
        [HttpPost]
        [Route("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var shipping = await _dataContext.Shippings.FindAsync(id);
                if (shipping == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa chỉ" });
                }

                _dataContext.Shippings.Remove(shipping);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa địa chỉ" });
            }
        }

    }
}
