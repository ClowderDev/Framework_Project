using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Framework_Project.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using Framework_Project.Areas.Admin.Repository;
using Microsoft.AspNetCore.Authentication.Google;

namespace Framework_Project.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(UserManager<AppUserModel> userManage,
            SignInManager<AppUserModel> signInManager, DataContext context, IEmailSender emailSender)
        {
            _userManage = userManage;
            _signInManager = signInManager;
            _dataContext = context;
            _emailSender = emailSender;
        }
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                // Thực hiện đăng nhập bằng mật khẩu.
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {

                    // Chuyển hướng đến URL ban đầu hoặc trang chủ nếu không có returnUrl.
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            }
            return View(loginVM);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
                // Tạo người dùng mới với mật khẩu.
                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo thành viên thành công";
                    return Redirect("/account/login");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }


        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            // Đăng xuất nếu đăng nhập bằng mật khẩu
            await _signInManager.SignOutAsync();
            // Đăng xuất nếu đăng nhập bằng Google
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            // Tìm người dùng theo email.
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgotPass", "Account");
            }
            else
            {
                // Tạo token ngẫu nhiên.
                string token = Guid.NewGuid().ToString();
                // Cập nhật token cho người dùng.
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Change password for user " + checkMail.Email;
                var message = "Click on link to change password " +
                    $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token + "";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }


            TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
            return RedirectToAction("ForgotPass", "Account");
        }
        public IActionResult ForgotPass()
        {
            return View();
        }
        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            // Tìm người dùng theo email và token.
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                // Truyền email và token đến View.
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            // Tìm người dùng theo email và token.
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                // Tạo token mới sau khi cập nhật mật khẩu.
                string newtoken = Guid.NewGuid().ToString();
                // Hash mật khẩu mới.
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                // Cập nhật mật khẩu và token mới cho người dùng.
                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManage.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
            }
            // Lấy ID và Email của người dùng hiện tại.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Lấy danh sách đơn hàng của người dùng, sắp xếp theo ID giảm dần.
            var Orders = await _dataContext.Orders
                .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
            // Truyền email người dùng đến View.
            ViewBag.UserEmail = userEmail;
            // Trả về View với danh sách đơn hàng.
            return View(Orders);
        }

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account");
            }
            try
            {
                // Tìm đơn hàng theo mã đơn hàng.
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                // Cập nhật trạng thái đơn hàng thành 3 (đã hủy).
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                return BadRequest("An error occurred while canceling the order.");
            }


            return RedirectToAction("History", "Account");
        }

        public async Task LoginByGoogle()
        {
            // Sử dụng Google authentication scheme
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        // Xử lý phản hồi từ Google sau khi đăng nhập.
        // Xác thực người dùng và tạo tài khoản mới nếu chưa tồn tại.
        public async Task<IActionResult>GoogleResponse()
        {
            // Xác thực bằng Google scheme.
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                //Nếu xác thực ko thành công quay về trang Login
                return RedirectToAction("Login");
            }

            // Lấy thông tin claims từ kết quả xác thực.
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            // Lấy email từ claims.
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            // Lấy phần tên của email để làm username.
            string emailName = email.Split('@')[0];

            // Kiểm tra xem người dùng có tồn tại trong database không.
            var existingUser = await _userManage.FindByEmailAsync(email);

            if (existingUser == null)
            {
                // Nếu người dùng không tồn tại, tạo người dùng mới với mật khẩu hashed mặc định.
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "test123");
                // Tạo đối tượng AppUserModel mới.
                var newUser = new AppUserModel { UserName = emailName, Email = email };
                // Gán mật khẩu đã hash cho người dùng mới.
                newUser.PasswordHash = hashedPassword; 

                // Tạo người dùng trong Identity system.
                var createUserResult = await _userManage.CreateAsync(newUser);
                if (!createUserResult.Succeeded)
                {
                    TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                    return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

                }
                else
                {
                    // Nếu tạo người dùng thành công, đăng nhập cho người dùng đó.
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Đăng ký tài khoản thành công.";
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                // Nếu người dùng đã tồn tại, đăng nhập cho người dùng đó.
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
            }

            return RedirectToAction("Login");

        }

    }
}
