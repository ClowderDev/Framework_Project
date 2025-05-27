using Framework_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Framework_Project.Repository
{
    public class SeedData
    {
        public static async Task SeedingData(DataContext _context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Tạo database
            _context.Database.Migrate();

            string adminRole = "Admin";
            string userRole = "User";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }
            if (!await roleManager.RoleExistsAsync(userRole))
            {
                await roleManager.CreateAsync(new IdentityRole(userRole));
            }
            
            // Get role IDs
            var adminRoleEntity = await roleManager.FindByNameAsync(adminRole);
            var userRoleEntity = await roleManager.FindByNameAsync(userRole);
            
            string adminRoleId = adminRoleEntity?.Id;
            string userRoleId = userRoleEntity?.Id;

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new AppUserModel
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    Occuapation = "System Administrator",
                    RoleId = adminRoleId
                };
                var result = await userManager.CreateAsync(adminUser, "test123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }

            if (await userManager.FindByNameAsync("user1") == null)
            {
                var user1 = new AppUserModel
                {
                    UserName = "user1",
                    Email = "user1@example.com",
                    EmailConfirmed = true,
                    Occuapation = "Customer",
                    RoleId = userRoleId
                };
                var result = await userManager.CreateAsync(user1, "test123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user1, userRole);
                }
            }

            if (await userManager.FindByNameAsync("user2") == null)
            {
                var user2 = new AppUserModel
                {
                    UserName = "user2",
                    Email = "user2@example.com",
                    EmailConfirmed = true,
                    Occuapation = "Tester",
                    RoleId = userRoleId
                };
                var result = await userManager.CreateAsync(user2, "test123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user2, userRole);
                }
            }

            if (!_context.Categories.Any())
            {
                var categoryLaptop = new CategoryModel { Name = "Laptops", Slug = "laptops", Description = "High-performance laptops", Status = 1 };
                var categorySmartphone = new CategoryModel { Name = "Smartphones", Slug = "smartphones", Description = "Latest smartphones", Status = 1 };
                var categoryAccessories = new CategoryModel { Name = "Accessories", Slug = "accessories", Description = "Computer and phone accessories", Status = 1 };
                _context.Categories.AddRange(categoryLaptop, categorySmartphone, categoryAccessories);
                await _context.SaveChangesAsync(); 
            }

            if (!_context.Brands.Any())
            {
                var brandApple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Apple Inc.", Status = 1 };
                var brandSamsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "Samsung Electronics", Status = 1 };
                var brandDell = new BrandModel { Name = "Dell", Slug = "dell", Description = "Dell Technologies", Status = 1 };
                var brandLogitech = new BrandModel { Name = "Logitech", Slug = "logitech", Description = "Logitech International S.A.", Status = 1 };
                _context.Brands.AddRange(brandApple, brandSamsung, brandDell, brandLogitech);
                await _context.SaveChangesAsync(); 
            }

            if (!_context.Products.Any())
            {
                var categoryLaptop = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "laptops");
                var categorySmartphone = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "smartphones");
                var categoryAccessories = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "accessories");

                var brandApple = await _context.Brands.FirstOrDefaultAsync(b => b.Slug == "apple");
                var brandSamsung = await _context.Brands.FirstOrDefaultAsync(b => b.Slug == "samsung");
                var brandDell = await _context.Brands.FirstOrDefaultAsync(b => b.Slug == "dell");
                var brandLogitech = await _context.Brands.FirstOrDefaultAsync(b => b.Slug == "logitech");

                if (categoryLaptop != null && brandApple != null)
                {
                    _context.Products.Add(new ProductModel { Name = "MacBook Pro 16-inch", Slug = "macbook-pro-16", Description = "The ultimate pro laptop.", Image = "macbook_pro_16.jpg", CategoryId = categoryLaptop.Id, BrandId = brandApple.Id, Price = 2399999.00m, Quantity = 50 });
                }
                if (categoryLaptop != null && brandDell != null)
                {
                    _context.Products.Add(new ProductModel { Name = "Dell XPS 15", Slug = "dell-xps-15", Description = "Stunning display, powerful performance.", Image = "dell_xps_15.jpg", CategoryId = categoryLaptop.Id, BrandId = brandDell.Id, Price = 189999999.00m, Quantity = 75 });
                }
                if (categorySmartphone != null && brandApple != null)
                {
                    _context.Products.Add(new ProductModel { Name = "iPhone 15 Pro", Slug = "iphone-15-pro", Description = "The latest iPhone with Pro features.", Image = "iphone_15_pro.jpg", CategoryId = categorySmartphone.Id, BrandId = brandApple.Id, Price = 99999999.00m, Quantity = 150 });
                }
                if (categorySmartphone != null && brandSamsung != null)
                {
                    _context.Products.Add(new ProductModel { Name = "Samsung Galaxy S24 Ultra", Slug = "galaxy-s24-ultra", Description = "Experience the epic standard.", Image = "galaxy_s24_ultra.jpg", CategoryId = categorySmartphone.Id, BrandId = brandSamsung.Id, Price = 119999999.00m, Quantity = 120 });
                }
                if (categoryAccessories != null && brandLogitech != null)
                {
                    _context.Products.Add(new ProductModel { Name = "Logitech MX Master 3S", Slug = "logitech-mx-master-3s", Description = "Advanced wireless mouse.", Image = "mx_master_3s.jpg", CategoryId = categoryAccessories.Id, BrandId = brandLogitech.Id, Price = 9999999.00m, Quantity = 200 });
                }
                 if (categoryAccessories != null && brandApple != null)
                {
                    _context.Products.Add(new ProductModel { Name = "Apple Magic Keyboard", Slug = "apple-magic-keyboard", Description = "Wireless keyboard with numeric keypad.", Image = "magic_keyboard.jpg", CategoryId = categoryAccessories.Id, BrandId = brandApple.Id, Price = 12999999.00m, Quantity = 90 });
                }
                await _context.SaveChangesAsync();
            }

            if (!_context.Sliders.Any())
            {
                _context.Sliders.AddRange(
                    new SliderModel { Name = "Slider 1", Description = "Discover our new arrivals", Image = "slider1.jpg", Status = 1 },
                    new SliderModel { Name = "Slider 2", Description = "Special offers this week", Image = "slider2.jpg", Status = 1 },
                    new SliderModel { Name = "Slider 3", Description = "Top tech deals", Image = "slider3.jpg", Status = 1 }
                );
                await _context.SaveChangesAsync();
            }

        }
    }
}