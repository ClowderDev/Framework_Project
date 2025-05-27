using System.Security.Claims;
using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
        }
            
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Truy vấn để lấy danh sách user và role của user đó
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId into userRolesGroup
                                        from ur in userRolesGroup.DefaultIfEmpty()
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id into rolesGroup
                                        from r in rolesGroup.DefaultIfEmpty()
                                        select new { User = u, RoleName = r.Name})
                               .ToListAsync();

            // Lấy ID của người dùng hiện đang đăng nhập
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Lưu ID người dùng đăng nhập vào ViewBag để sử dụng trong View
            ViewBag.LoggedInUserId = loggedInUserId;
            // Trả về View với danh sách người dùng và vai trò
            return View(usersWithRoles);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách tất cả các vai trò từ RoleManager
            var roles = await _roleManager.Roles.ToListAsync();
            // Tạo SelectList cho dropdown chọn vai trò trong View
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            // Trả về View với một đối tượng AppUserModel mới để điền thông tin
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                // Tạo người dùng mới bằng UserManager
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                // Kiểm tra kết quả tạo người dùng
                if (createUserResult.Succeeded)
                {
                    // Tìm người dùng vừa tạo dựa trên email
                    var createUser = await _userManager.FindByEmailAsync(user.Email);
                    // Lấy ID của người dùng vừa tạo
                    var userId = createUser.Id;
                    // Tìm vai trò dựa trên RoleId được chọn từ form
                    var role = _roleManager.FindByIdAsync(user.RoleId);
                    // Gán vai trò cho người dùng vừa tạo
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    // Kiểm tra kết quả gán vai trò
                    if (!addToRoleResult.Succeeded)
                    {
                        // Nếu gán vai trò thất bại, thêm các lỗi vào ModelState
                        foreach (var error in createUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }

                    // Chuyển hướng về trang Index sau khi tạo thành công
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    // Nếu tạo người dùng thất bại, thêm các lỗi vào ModelState
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // Trả về View với dữ liệu người dùng đã nhập để hiển thị lỗi
                    return View(user);
                }

            }
            else
            {
                // Nếu Model không hợp lệ, hiển thị thông báo lỗi
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                // Lấy tất cả các lỗi từ ModelState
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                // Gom các lỗi thành một chuỗi và trả về BadRequest
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            // Lấy lại danh sách vai trò để hiển thị lại dropdown nếu có lỗi
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            // Trả về View với dữ liệu người dùng đã nhập
            return View(user);

        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            //Lấy thông tin user dựa trên id
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Model validation failed.";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }
        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            TempData["success"] = "User đã được xóa thành công";
            return RedirectToAction("Index");
        }

    }
}
