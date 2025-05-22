using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            CategoryModel category = _dataContext.Categories.Where(c => c.Slug == slug).FirstOrDefault();


            if (category == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Slug = slug;
            IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryId == category.Id);
            var count = await productsByCategory.CountAsync();
            
 
            if (sort_by == "price_increase")
            {
                productsByCategory = productsByCategory.OrderBy(p => p.Price);
            }
            else if (sort_by == "price_decrease")
            {
                productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
            }
            else if (sort_by == "price_newest")
            {
                productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
            }
            else if (sort_by == "price_oldest")
            {
                productsByCategory = productsByCategory.OrderBy(p => p.Id);
            }

            decimal startPriceValue = 50000;
            decimal endPriceValue = 5000000;

            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                {
                    productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }
            else
            {
                productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
            }

            ViewBag.startprice = startPriceValue;
            ViewBag.endprice = endPriceValue;

            ViewBag.sort_key = sort_by;
            ViewBag.count = count; 

            decimal minPrice = 50000;
            decimal maxPrice = 5000000;

            if (count > 0)
            {
                var prices = await productsByCategory.Select(p => p.Price).ToListAsync();
                if (prices.Any())
                {
                    minPrice = prices.Min();
                    maxPrice = prices.Max();
                }
            }

            ViewBag.minprice = minPrice;
            ViewBag.maxprice = maxPrice;

            return View(await productsByCategory.ToListAsync());
        }
    }
}
