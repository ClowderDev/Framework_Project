using Framework_Project.Models;
using Framework_Project.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Contact")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ContactController(DataContext context, IWebHostEnvironment webHostEnviroment)
        {
            _dataContext = context;
            _webHostEnviroment = webHostEnviroment;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            var contact = _dataContext.Contact.ToList();
            return View(contact);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactModel contact)
        {
            if (ModelState.IsValid)
            {
                if (contact.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    contact.LogoImg = imageName;
                }

                _dataContext.Add(contact);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thông tin web thành công";
                return RedirectToAction("Index");
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
                return BadRequest(errorMessage);
            }
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contact.FirstOrDefaultAsync();
            return View(contact);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var existed_contact = _dataContext.Contact.FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (contact.ImageUpload != null)
                {
                    // Kết hợp đường dẫn gốc của ứng dụng với thư mục lưu ảnh logo
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/logo");
                    // Tạo tên file duy nhất bằng GUID và tên file gốc
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    // Kết hợp đường dẫn thư mục và tên file để có đường dẫn đầy đủ
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // Tạo FileStream để ghi file
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    // Sao chép nội dung file tải lên vào FileStream 
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    // Cập nhật tên file ảnh logo mới vào đối tượng contact hiện có
                    existed_contact.LogoImg = imageName;
                }

                existed_contact.Name = contact.Name;
                existed_contact.Email = contact.Email;
                existed_contact.Description = contact.Description;
                existed_contact.Phone = contact.Phone;
                existed_contact.Map = contact.Map;


                _dataContext.Update(existed_contact);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin web thành công";
                return RedirectToAction("Index");

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
                return BadRequest(errorMessage);
            }
            return View(contact);
        }

        [HttpPost]
        [Route("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _dataContext.Contact.FindAsync(id);
            if (contact == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin liên hệ." });
            }

            try
            {
                // Optional: Delete associated image file if it exists
                if (!string.IsNullOrEmpty(contact.LogoImg))
                {
                    string imagePath = Path.Combine(_webHostEnviroment.WebRootPath, "media/logo", contact.LogoImg);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _dataContext.Contact.Remove(contact);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true, message = "Thông tin liên hệ đã được xóa thành công." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                // _logger.LogError(ex, "Error deleting contact with ID {ContactId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa thông tin liên hệ." });
            }
        }
    }
}
