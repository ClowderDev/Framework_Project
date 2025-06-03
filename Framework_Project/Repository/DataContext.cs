using Framework_Project.Models;
using Framework_Project.Models.Momo;
using Framework_Project.Models.Vnpay;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
  

namespace Framework_Project.Repository
{ 

    public class DataContext : IdentityDbContext<AppUserModel>
    {
        // Constructor nhận DbContextOptions để cấu hình kết nối database.
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContactModel>().HasData(
                new ContactModel
                {
                    Id = 1,
                    Name = "Framework Shop",
                    Map = "<iframe src=\"https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d13178.937968367609!2d106.80472990520988!3d10.878347242885324!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x317527587e9ad5bf%3A0xafa66f9c8be3c91!2sUniversity%20of%20Information%20Technology%20-%20VNUHCM!5e0!3m2!1sen!2s!4v1747905663960!5m2!1sen!2s\" width=\"400\" height=\"300\" style=\"border:0;\" allowfullscreen=\"\" loading=\"lazy\" referrerpolicy=\"no-referrer-when-downgrade\"></iframe>",
                    Email = "contact@frameworkshop.com",
                    Phone = "+1 234 567 890",
                    Description = "Welcome to Framework Shop - Your one-stop destination for quality products.",
                    LogoImg = "eb7ec5ad-9a7c-4f0d-b744-c4edd6d7387e_logo.jpg"
                }
            );
        }

        public DbSet<BrandModel> Brands { get; set; }

        public DbSet<SliderModel> Sliders { get; set; }

        public DbSet<ProductModel> Products { get; set; }

        public DbSet<RatingModel> Ratings { get; set; }

        public DbSet<CategoryModel> Categories { get; set; }

        public DbSet<OrderModel> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<ContactModel> Contact { get; set; }

        public DbSet<WishlistModel> Wishlists { get; set; }

        public DbSet<CompareModel> Compares { get; set; }

        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }

        public DbSet<ShippingModel> Shippings { get; set; }

        public DbSet<CouponModel> Coupons { get; set; }

        public DbSet<StatisticalModel> Statisticals { get; set; }

        public DbSet<MomoInfo> MomoInfos { get; set; }

        public DbSet<VnpayModel> VnpayModels { get; set; }


    }
}
