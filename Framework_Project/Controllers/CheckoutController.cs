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

				// Retrieve shipping price from cookie
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;

				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}
				orderItem.ShippingCost = shippingPrice;

				// Get cart items and calculate totals
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				decimal subtotal = cartItems.Sum(x => x.Quantity * x.Price);

				// Get coupon data from cookie
				var couponDataCookie = Request.Cookies["CouponData"];
				decimal discountAmount = 0;
				if (couponDataCookie != null)
				{
					try
					{
						var couponData = JsonConvert.DeserializeObject<dynamic>(couponDataCookie);
						double discountPercentage = couponData.DiscountPercentage;
						discountAmount = subtotal * (decimal)(discountPercentage / 100);
						orderItem.CouponCode = couponData.Name;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error deserializing coupon cookie: {ex.Message}");
						Response.Cookies.Delete("CouponData");
					}
				}

				orderItem.DiscountAmount = discountAmount;
				orderItem.TotalAmount = subtotal - discountAmount + shippingPrice;

				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();

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
				var newMomoInfo = new MomoInfo(){
					OrderId = requestQuery["orderId"],
					FullName = User.FindFirstValue(ClaimTypes.Email),
					Amount = decimal.Parse(requestQuery["Amount"]),
					OrderInfo = requestQuery["orderInfo"],
					DatePaid = DateTime.Now
				};
				_dataContext.Add(newMomoInfo);
				await _dataContext.SaveChangesAsync();
				await Checkout(requestQuery["orderId"], "Momo");
			}
			else{
				TempData["error"] = "Thanh toán thất bại, vui lòng thử lại.";	
				return RedirectToAction("Index", "Cart");
			}

			
			return View(response);
		}

		[HttpGet]
		public async Task<IActionResult> PaymentCallbackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);
			if(response.VnPayResponseCode == "00"){
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
				await Checkout(response.OrderId, "VNPay");
			}
			else{
				TempData["error"] = "Thanh toán thất bại, vui lòng thử lại.";
				return RedirectToAction("Index", "Cart");	
			}

			return View(response);
		}
	}

}
