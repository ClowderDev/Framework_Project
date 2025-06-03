using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();

            var Order = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();

            // Lưu chi phí vận chuyển và trạng thái đơn hàng vào ViewBag để truyền sang View
            ViewBag.ShippingCost = Order.ShippingCost;
            ViewBag.Status = Order.Status;
            return View(DetailsOrder);
        }
        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật đơn hàng thành công" });
            }
            catch (Exception)
            {


                return StatusCode(500, "Xảy ra lỗi khi cập nhật đơn hàng");
            }
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            try
            {
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return StatusCode(500, "Xảy ra lỗi khi cập nhật đơn hàng");
            }
        }

    }
}
