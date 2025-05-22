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

        public async Task<IActionResult> Index(string slug = "") 
        {

            BrandModel brand = _dataContext.Brands.Where(c => c.Slug == slug).FirstOrDefault();

            if (brand == null) 
            {
                return RedirectToAction("Index");
            }


            var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
            ViewBag.Slug = slug; 
            return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
