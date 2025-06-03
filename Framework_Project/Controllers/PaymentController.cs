using Framework_Project.Models;
using Framework_Project.Models.Vnpay;
using Framework_Project.Services.Momo;
using Framework_Project.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Framework_Project.Repository;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Framework_Project.Controllers
{
    public class PaymentController : Controller
	{
		private readonly IVnPayService _vnPayService;
		private IMomoService _momoService;
		private readonly DataContext _dataContext;
		public PaymentController(IMomoService momoService, IVnPayService vnPayService, DataContext dataContext)
		{
			_momoService = momoService;
			_vnPayService = vnPayService;
			_dataContext = dataContext;
		}

		private string CreatePendingOrder(HttpContext httpContext, string paymentMethod, string userEmail, decimal amount)
		{
			// Generate order code
			var orderCode = Guid.NewGuid().ToString();
			// Get cart items
			var cartItems = httpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			// Get coupon info
			string couponCode = null;
			double discountPercentage = 0;
			var couponDataCookie = httpContext.Request.Cookies["CouponData"];
			if (couponDataCookie != null)
			{
				try
				{
					var couponData = JsonConvert.DeserializeObject<dynamic>(couponDataCookie);
					couponCode = couponData.Name;
					discountPercentage = couponData.DiscountPercentage;
				}
				catch { }
			}
			// Get shipping price
			decimal shippingPrice = 0;
			var shippingPriceCookie = httpContext.Request.Cookies["ShippingPrice"];
			if (shippingPriceCookie != null)
			{
				try
				{
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
				}
				catch { }
			}
			// Calculate totals
			decimal totalAmount = cartItems.Sum(x => x.Quantity * x.Price);
			decimal discountAmount = 0;
			if (discountPercentage > 0)
			{
				discountAmount = totalAmount * (decimal)discountPercentage / 100;
			}
			// Create order
			var orderItem = new OrderModel
			{
				OrderCode = orderCode,
				UserName = userEmail,
				Status = 1, // pending
				CreatedDate = DateTime.Now,
				PaymentMethod = paymentMethod,
				CouponCode = couponCode,
				ShippingCost = shippingPrice,
				DiscountAmount = discountAmount,
				TotalAmount = totalAmount - discountAmount + shippingPrice
			};
			_dataContext.Add(orderItem);
			_dataContext.SaveChanges();
			// Create order details
			foreach (var cart in cartItems)
			{
				var orderdetail = new OrderDetail
				{
					UserName = userEmail,
					OrderCode = orderCode,
					ProductId = cart.ProductId,
					Price = cart.Price,
					Quantity = cart.Quantity
				};
				_dataContext.Add(orderdetail);
			}
			_dataContext.SaveChanges();
			return orderCode;
		}

		[HttpPost]
		public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
		{
			try
			{
				if (model == null)
				{
					return BadRequest("Invalid order information");
				}
				var userEmail = User.FindFirstValue(ClaimTypes.Email);
				// Create pending order and get order code
				var orderCode = CreatePendingOrder(HttpContext, "Momo", userEmail, (decimal)model.Amount);
				model.OrderId = orderCode;
				var response = await _momoService.CreatePaymentAsync(model);
				
				if (response == null || string.IsNullOrEmpty(response.PayUrl))
				{
					return BadRequest("Failed to create payment URL");
				}

				return Redirect(response.PayUrl);
			}
			catch (Exception ex)
			{
				// Log the exception here
				return BadRequest($"Payment creation failed: {ex.Message}");
			}
		}

		[HttpGet]
		public async Task<IActionResult> PaymentCallBack()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			if (response != null && response.OrderId != null)
			{
				// Find order by OrderCode only
				var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == response.OrderId);
				if (order != null)
				{
					order.Status = 1;
					order.PaymentMethod = "Momo";
					_dataContext.Update(order);
					await _dataContext.SaveChangesAsync();
				}
			}
			return View(response);
		}

		[HttpPost]
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			// Create pending order and get order code
			var orderCode = CreatePendingOrder(HttpContext, "VNPay", userEmail, (decimal)model.Amount);
			model.OrderId = orderCode;
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
			return Redirect(url);
		}

		[HttpGet]
		public async Task<IActionResult> PaymentCallBackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);
			if (response != null && response.OrderId != null && response.VnPayResponseCode == "00")
			{
				// Find order by OrderCode only
				var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == response.OrderId);
				if (order != null)
				{
					order.Status = 1;
					order.PaymentMethod = "VNPay";
					_dataContext.Update(order);
					await _dataContext.SaveChangesAsync();
				}
			}
			return View(response);
		}
	}
}
