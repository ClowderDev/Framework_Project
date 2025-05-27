using Framework_Project.Areas.Admin.Repository;
using Framework_Project.Models;
using Framework_Project.Models.Momo;
using Framework_Project.Repository;
using Framework_Project.Services.Momo;
using Framework_Project.Services.Vnpay;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);



//Kết nối Momo
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Thêm SQL Server cho web
// Chuỗi kết nối đến SQL Server được lấy từ file appsettings.json
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});

// Thêm email
// Transient để tạo mới instance mỗi request
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Thêm model MVC
builder.Services.AddControllersWithViews();

// Cấu hình dùng session
builder.Services.AddDistributedMemoryCache();

// Cấu hình session sống 30p
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// Cấu hình identity với custom user và role 
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

// Setup đăng nhập bằng google và cookie
builder.Services.AddAuthentication(options =>
{
    //Cấu hình đăng nhập bằng cookie, mặc định đã có sẵn
    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    //ClientId và SecretKey lấy từ file appsettings.json
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

// Cấu hình chi tiết đăng nhập bằng cookie  
// Đăng nhập thành công sẽ chuyển đến trang Home/Index
// Đăng nhập thất bại sẽ chuyển đến trang Account/Login
// Thời gian tồn tại của cookie là 60p
// Đăng nhập thành công sẽ cập nhật thời gian tồn tại của cookie
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.AddRazorPages();


builder.Services.Configure<IdentityOptions>(options =>
{
    // Cài đặt yêu cầu về mật khẩu
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;

    // Yêu cầu 1 người dùng có 1 email
    options.User.RequireUniqueEmail = true;

    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
});

//Kết nối VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();

var app = builder.Build();

// Cài đặt trang lỗi tùy chỉnh dựa trên mã  HTTP
app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

// Kích hoạt sử dụng middleware session
app.UseSession();

// Cấu hình xử lý ngoại lệ 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Thứ tự thực hiện luồng đăng nhập rồi mới cho phép truy cập
app.UseAuthentication();

// Kích hoạt ủy quyền (phân quyền)
app.UseAuthorization();

// Middleware tùy chỉnh để chuyển hướng route mặc định /Admin về trang Dashboard
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/Admin", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/Admin/Dashboard/");
        return;
    }
    await next();
});

// Cấu hình các route cho ứng dụng
// Route cho danh mục sản phẩm, sử dụng slug
app.MapControllerRoute(
    name: "category",
    pattern: "/category/{Slug?}",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "brand",
    pattern: "/brand/{Slug?}",
    defaults: new { controller = "Brand", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Areas",
    pattern: "{areas:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Seed data mặc định để test
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUserModel>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.SeedingData(context, userManager, roleManager);
}

app.Run();
