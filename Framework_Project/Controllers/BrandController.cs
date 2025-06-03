using Framework_Project.Models;
using Framework_Project.Repository; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 

namespace Framework_Project.Controllers 
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext; 

        public BrandController(DataContext dataContext) 
        {
            _dataContext = dataContext; 
        }

        public async Task<IActionResult> Index(string slug = "", string sort_by = "", string startprice = "", string endprice = "") 
        {
            BrandModel brand = _dataContext.Brands.Where(c => c.Slug == slug).FirstOrDefault();

            if (brand == null) 
            {
                return RedirectToAction("Index");
            }

            ViewBag.BrandSlug = slug;
            IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
            var count = await productsByBrand.CountAsync();

            if (sort_by == "price_increase")
            {
                productsByBrand = productsByBrand.OrderBy(p => p.Price);
            }
            else if (sort_by == "price_decrease")
            {
                productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
            }
            else if (sort_by == "price_newest")
            {
                productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
            }
            else if (sort_by == "price_oldest")
            {
                productsByBrand = productsByBrand.OrderBy(p => p.Id);
            }

            decimal startPriceValue = 50000;
            decimal endPriceValue = 1000000000;

            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                {
                    productsByBrand = productsByBrand.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }
            else
            {
                productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
            }

            ViewBag.startprice = startPriceValue;
            ViewBag.endprice = endPriceValue;
            ViewBag.sort_key = sort_by;
            ViewBag.count = count;

            decimal minPrice = 50000;
            decimal maxPrice = 1000000000;

            if (count > 0)
            {
                var prices = await productsByBrand.Select(p => p.Price).ToListAsync();
                if (prices.Any())
                {
                    minPrice = prices.Min();
                    maxPrice = prices.Max();
                }
            }

            ViewBag.minprice = minPrice;
            ViewBag.maxprice = maxPrice;

            return View(await productsByBrand.ToListAsync());
        }
    }
}
