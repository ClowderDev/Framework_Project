using Framework_Project.Models;
using Framework_Project.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Framework_Project.Areas.Admin.Repository;
using Framework_Project.Services.Vnpay;
using Framework_Project.Models.Momo;
using Framework_Project.Services.Momo;
using Framework_Project.Models.Vnpay;
using System.Transactions;
using Framework_Project.Models.Vnpay;
using Framework_Project.Models.Momo;

namespace Framework_Project.Controllers
{
    public class CheckoutController : Controller
	{

		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		private readonly IVnPayService _vnPayService;
		private readonly IMomoService _momoService;
		private static readonly HttpClient client = new HttpClient();
		public CheckoutController(IMomoService momoService, IEmailSender emailSender, DataContext context, IVnPayService vnPayService)
		{
			_dataContext = context;
			_emailSender = emailSender;
			_vnPayService = vnPayService;
			_momoService = momoService;

		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Checkout(string OrderId, string paymentMethod)
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var ordercode = !string.IsNullOrEmpty(OrderId) ? OrderId : Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = ordercode;
				orderItem.UserName = userEmail;
				orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;

				if(paymentMethod == "Momo"){
					orderItem.PaymentMethod = "Momo";	
				}
				else if(paymentMethod == "VNPay"){
					orderItem.PaymentMethod = "VNPay";
				}
				else{
					orderItem.PaymentMethod = "COD";
				}

				// Get cart items
				var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				if (cartItems.Count == 0)
				{
					return RedirectToAction("Index", "Cart");
				}

				// Get coupon information from cookie
				string couponCode = null;
				double discountPercentage = 0;
				var couponDataCookie = Request.Cookies["CouponData"];
				if (couponDataCookie != null)
				{
					try
					{
						var couponData = JsonConvert.DeserializeObject<dynamic>(couponDataCookie);
						couponCode = couponData.Name;
						discountPercentage = couponData.DiscountPercentage;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error deserializing coupon cookie: {ex.Message}");
					}
				}

				// Calculate total amount and discount
				decimal totalAmount = cartItems.Sum(x => x.Quantity * x.Price);
				decimal discountAmount = 0;
				if (discountPercentage > 0)
				{
					discountAmount = totalAmount * (decimal)discountPercentage / 100;
				}

				// Get shipping price from cookie
				decimal shippingPrice = 0;
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				if (shippingPriceCookie != null)
				{
					try
					{
						shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error deserializing shipping price cookie: {ex.Message}");
					}
				}

				// Set order details
				orderItem.TotalAmount = totalAmount - discountAmount + shippingPrice;
				orderItem.ShippingCost = shippingPrice;
				orderItem.CouponCode = couponCode;
				orderItem.DiscountAmount = discountAmount;

				_dataContext.Add(orderItem);
				await _dataContext.SaveChangesAsync();

				// Create order details
				foreach (var cart in cartItems)
				{
					var orderdetail = new OrderDetail();
					orderdetail.UserName = userEmail;
					orderdetail.OrderCode = ordercode;
					orderdetail.ProductId = cart.ProductId;
					orderdetail.Price = cart.Price;
					orderdetail.Quantity = cart.Quantity;
					//update product quantity
					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.SoldOut += cart.Quantity;
					_dataContext.Update(product);
					_dataContext.Add(orderdetail);
					_dataContext.SaveChanges();
				}

				HttpContext.Session.Remove("Cart");
				Response.Cookies.Delete("CouponData");
				Response.Cookies.Delete("ShippingPrice");

				//Gửi mail khi đặt hàng thành công
				var receiver = userEmail;
				var subject = "Đặt hàng thành công";
				var message = "Đặt hàng thành công, cảm ơn bạn đã ủng hộ";

				await _emailSender.SendEmailAsync(receiver, subject, message);

				TempData["success"] = "Đơn hàng đã được tạo,vui lòng chờ duyệt đơn hàng nhé.";
				return RedirectToAction("History", "Account");
			}
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> PaymentCallback(MomoInfo model)
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			var requestQuery = HttpContext.Request.Query;
			if(requestQuery["resultCode"] != "0"){
				var orderId = requestQuery["orderId"].ToString();
				var order = await _dataContext.Set<OrderModel>().FirstOrDefaultAsync(o => o.OrderCode == orderId);
				if (order != null)
				{
					order.Status = 1; // Paid/Confirmed
					order.PaymentMethod = "Momo";
					_dataContext.Update(order);
					await _dataContext.SaveChangesAsync();
				}
				var newMomoInfo = new MomoInfo(){
					OrderId = orderId,
					FullName = User.FindFirstValue(ClaimTypes.Email),
					Amount = decimal.Parse(requestQuery["Amount"]),
					OrderInfo = requestQuery["orderInfo"],
					DatePaid = DateTime.Now
				};
				_dataContext.Add(newMomoInfo);
				await _dataContext.SaveChangesAsync();
				// Clean up session/cookies
				HttpContext.Session.Remove("Cart");
				Response.Cookies.Delete("CouponData");
				Response.Cookies.Delete("ShippingPrice");
				// Send email
				var receiver = User.FindFirstValue(ClaimTypes.Email);
				var subject = "Đặt hàng thành công";
				var message = "Đặt hàng thành công, cảm ơn bạn đã ủng hộ";
				await _emailSender.SendEmailAsync(receiver, subject, message);
				TempData["success"] = "Đơn hàng đã được tạo,vui lòng chờ duyệt đơn hàng nhé.";
				return RedirectToAction("History", "Account");
			}
			else{
				TempData["error"] = "Thanh toán thất bại, vui lòng thử lại.";	
				return RedirectToAction("Index", "Cart");
			}
		}

		[HttpGet]
		public async Task<IActionResult> PaymentCallbackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);
			if(response.VnPayResponseCode == "00"){
				var orderId = response.OrderId;
				var order = await _dataContext.Set<OrderModel>().FirstOrDefaultAsync(o => o.OrderCode == orderId);
				if (order != null)
				{
					order.Status = 1; // Paid/Confirmed
					order.PaymentMethod = "VNPay";
					_dataContext.Update(order);
					await _dataContext.SaveChangesAsync();
				}
				var newVnpayInsert = new VnpayModel{
					OrderId = response.OrderId,
					PaymentMethod = "VNPay",
					OrderDescription = response.OrderDescription,
					TransactionId = response.TransactionId,
					PaymentId = response.PaymentId,
					DateCreated = DateTime.Now
				};
				_dataContext.Add(newVnpayInsert);
				await _dataContext.SaveChangesAsync();
				// Clean up session/cookies
				HttpContext.Session.Remove("Cart");
				Response.Cookies.Delete("CouponData");
				Response.Cookies.Delete("ShippingPrice");
				// Send email
				var receiver = User.FindFirstValue(ClaimTypes.Email);
				var subject = "Đặt hàng thành công";
				var message = "Đặt hàng thành công, cảm ơn bạn đã ủng hộ";
				await _emailSender.SendEmailAsync(receiver, subject, message);
				TempData["success"] = "Đơn hàng đã được tạo,vui lòng chờ duyệt đơn hàng nhé.";
				return RedirectToAction("History", "Account");
			}
			else{
				TempData["error"] = "Thanh toán thất bại, vui lòng thử lại.";
				return RedirectToAction("Index", "Cart");	
			}
		}
	}

}
