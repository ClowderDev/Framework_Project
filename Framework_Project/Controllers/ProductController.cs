using Framework_Project.Models;
using Framework_Project.Models.ViewModels;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Controllers
{
    public class ProductController : Controller
    {
        private DataContext _dataContext;

        public ProductController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            if(Id == null) return RedirectToAction("Index");

            var productsById = await _dataContext.Products
                .Include(p => p.Ratings)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Id == Id)
                .FirstOrDefaultAsync();

            if (productsById == null) return RedirectToAction("Index");

            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productsById,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .ToListAsync();

            ViewBag.Keyword = searchTerm;

            return View(products);
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if (!User.Identity.IsAuthenticated)
			{
				TempData["error"] = "Bạn cần đăng nhập để đánh giá sản phẩm.";
				return Redirect(Request.Headers["Referer"]);
			}

			// Always use the logged-in user's email
			var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value
				?? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			rating.Email = userEmail;

			if (ModelState.IsValid)
			{
				// Check if this user already reviewed this product
				var existingReview = await _dataContext.Ratings
					.FirstOrDefaultAsync(r => r.ProductId == rating.ProductId && r.Email == rating.Email);

				if (existingReview != null)
				{
					TempData["error"] = "Bạn đã đánh giá sản phẩm này rồi!";
					return Redirect(Request.Headers["Referer"]);
				}

				var ratingEntity = new RatingModel
				{
					ProductId = rating.ProductId,
					Name = rating.Name,
					Email = rating.Email,
					Comment = rating.Comment,
					Star = rating.Star
				};

				_dataContext.Ratings.Add(ratingEntity);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Thêm đánh giá thành công";
				return Redirect(Request.Headers["Referer"]);
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

				return RedirectToAction("Detail", new { id = rating.ProductId });
			}
		}

    }
}
