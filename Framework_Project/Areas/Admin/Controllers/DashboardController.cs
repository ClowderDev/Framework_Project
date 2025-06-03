using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {

        private readonly DataContext _dataContext;
        public DashboardController(DataContext context)
        {
            _dataContext = context;
        }
        public IActionResult Index()
        {
            // Lấy số lượng bản ghi từ các bảng Products, Orders, Categories, Users
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            // Lưu số lượng vào ViewBag để truyền dữ liệu sang View
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }
        [HttpPost]
        [Route("SubmitFilterDate")]
        public IActionResult SubmitFilterDate(string filterdate)
        {
            // Chuyển đổi chuỗi ngày nhận được thành đối tượng DateTime và lấy phần ngày
            var selectedDate = DateTime.Parse(filterdate).Date;
            // Tính ngày tiếp theo để xác định khoảng thời gian lọc (từ selectedDate đến trước nextDate)
            var nextDate = selectedDate.AddDays(1);
            // Truy vấn dữ liệu đơn hàng và chi tiết đơn hàng trong khoảng thời gian đã chọn
            var chartData = _dataContext.Orders
           .Where(o => o.CreatedDate >= selectedDate && o.CreatedDate < nextDate)
           .Join(_dataContext.OrderDetails,
              o => o.OrderCode,
              od => od.OrderCode,
              // Tạo đối tượng StatisticalModel với ngày, doanh thu và số lượng đơn hàng
              (o, od) => new StatisticalModel
              {
                  Date = o.CreatedDate,
                  Revenue = (od.Quantity * od.Price) - o.DiscountAmount,
                  Orders = 1
              })
          // Nhóm dữ liệu theo ngày
          .GroupBy(s => s.Date)
          // Chọn dữ liệu thống kê cho từng nhóm (tính tổng doanh thu và đếm số đơn hàng)
          .Select(group => new StatisticalModel
          {
              Date = group.Key,
              Revenue = group.Sum(s => s.Revenue),
              Orders = group.Count()
          })
          .ToList();

            // Kiểm tra nếu không có dữ liệu, thêm một bản ghi với giá trị 0 và thông báo
            if (!chartData.Any())
            {
                chartData.Add(new StatisticalModel
                {
                    Date = selectedDate,
                    Revenue = 0,
                    Orders = 0,
                    Message = "Không có dữ liệu cho ngày đã chọn"
                });
            }

            // Trả về dữ liệu dưới dạng JSON
            return Json(chartData);
        }
        [HttpPost]
        [Route("SelectFilterDate")]
        public IActionResult SelectFilterDate(string filterdate)
        {
            // Lấy ngày hiện tại
            var today = DateTime.Today;
            // Khai báo biến cho ngày bắt đầu và kết thúc của khoảng thời gian lọc
            var startDate = today;
            var endDate = today;
            // Khởi tạo danh sách chứa dữ liệu biểu đồ
            var chartData = new List<StatisticalModel>();

            // Sử dụng switch case để xác định khoảng thời gian dựa trên giá trị filterdate
            switch (filterdate)
            {
                case "this_month":
                    // Khoảng thời gian là tháng hiện tại
                    startDate = new DateTime(today.Year, today.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    break;

                case "last_month":
                    // Khoảng thời gian là tháng trước
                    startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    endDate = new DateTime(today.Year, today.Month, 1).AddDays(-1);
                    break;

                case "all_year":
                    // Khoảng thời gian là cả năm hiện tại
                    startDate = new DateTime(today.Year, 1, 1);
                    endDate = new DateTime(today.Year, 12, 31);
                    break;

                default:
                    // Trả về dữ liệu rỗng nếu giá trị filterdate không hợp lệ
                    return Json(new List<StatisticalModel>()); 
            }

            // Xử lý riêng cho trường hợp lọc cả năm
            if (filterdate == "all_year")
            {
                // Tạo danh sách cho tất cả các tháng trong năm
                for (int month = 1; month <= 12; month++)
                {
                    // Xác định ngày bắt đầu và kết thúc của từng tháng
                    var monthStart = new DateTime(today.Year, month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    // Truy vấn dữ liệu đơn hàng và chi tiết đơn hàng trong tháng hiện tại
                    var monthData = _dataContext.Orders
                        .Where(o => o.CreatedDate.Date >= monthStart.Date && o.CreatedDate.Date <= monthEnd.Date)
                        .Join(_dataContext.OrderDetails,
                            o => o.OrderCode,
                            od => od.OrderCode,
                            // Tạo đối tượng ẩn với doanh thu và số lượng đơn hàng
                            (o, od) => new
                            {
                                revenue = (od.Quantity * od.Price) - o.DiscountAmount,
                                orders = 1
                            })
                        .GroupBy(x => true)
                        // Chọn dữ liệu thống kê cho nhóm (tính tổng doanh thu và tổng số đơn hàng)
                        .Select(group => new StatisticalModel
                        {
                            Date = monthStart,
                            Revenue = group.Sum(s => s.revenue),
                            Orders = group.Sum(s => s.orders)
                        })
                        // Lấy kết quả đầu tiên hoặc tạo một StatisticalModel mới với giá trị 0 nếu không có dữ liệu
                        .FirstOrDefault() ?? new StatisticalModel
                        {
                            Date = monthStart,
                            Revenue = 0,
                            Orders = 0,
                            Message = $"Không có dữ liệu cho tháng {monthStart:MM/yyyy}"
                        };

                    // Thêm dữ liệu thống kê của tháng vào danh sách
                    chartData.Add(monthData);
                }
            }
            else
            {
                // Xử lý cho các trường hợp lọc theo tháng (tháng này, tháng trước)
                chartData = _dataContext.Orders
                    .Where(o => o.CreatedDate.Date >= startDate.Date && o.CreatedDate.Date <= endDate.Date)
                    // Join bảng Orders với OrderDetails dựa trên OrderCode
                    .Join(_dataContext.OrderDetails,
                        o => o.OrderCode,
                        od => od.OrderCode,
                        // Tạo đối tượng StatisticalModel với ngày, doanh thu và số lượng đơn hàng
                        (o, od) => new StatisticalModel
                        {
                            Date = o.CreatedDate,
                            Revenue = (od.Quantity * od.Price) - o.DiscountAmount,
                            Orders = 1
                        })
                    // Nhóm dữ liệu theo ngày
                    .GroupBy(s => s.Date)
                    // Chọn dữ liệu thống kê cho từng nhóm (tính tổng doanh thu và đếm số đơn hàng)
                    .Select(group => new StatisticalModel
                    {
                        Date = group.Key,
                        Revenue = group.Sum(s => s.Revenue),
                        Orders = group.Count()
                    })
                    .ToList();
            }

            // Kiểm tra nếu không có dữ liệu, thêm một bản ghi với giá trị 0 và thông báo
            if (!chartData.Any())
            {
                chartData.Add(new StatisticalModel
                {
                    Date = startDate,
                    Revenue = 0,
                    Orders = 0,
                    Message = "Không có dữ liệu cho khoảng thời gian đã chọn"
                });
            }

            // Trả về dữ liệu dưới dạng JSON
            return Json(chartData);
        }
        [HttpPost]
        [Route("GetChartData")]
        public IActionResult GetChartData()
        {
            var chartData = _dataContext.Orders
          .Join(_dataContext.OrderDetails,
              o => o.OrderCode,
              od => od.OrderCode,
              // Tạo đối tượng StatisticalModel với ngày, doanh thu và số lượng đơn hàng
              (o, od) => new StatisticalModel
              {
                  Date = o.CreatedDate,
                  Revenue = (od.Quantity * od.Price) - o.DiscountAmount,
                  Orders = 1
              })
          .GroupBy(s => s.Date)
          // Chọn dữ liệu thống kê cho từng nhóm (tính tổng doanh thu và đếm số đơn hàng)
          .Select(group => new StatisticalModel
          {
              Date = group.Key,
              Revenue = group.Sum(s => s.Revenue),
              Orders = group.Count()
          })
          .ToList();

            // Kiểm tra nếu không có dữ liệu, thêm một bản ghi với giá trị 0 và thông báo
            if (!chartData.Any())
            {
                chartData.Add(new StatisticalModel
                {
                    Date = DateTime.Today,
                    Revenue = 0,
                    Orders = 0,
                    Message = "Không có dữ liệu"
                });
            }

            // Trả về dữ liệu dưới dạng JSON
            return Json(chartData);
        }

    }
}
