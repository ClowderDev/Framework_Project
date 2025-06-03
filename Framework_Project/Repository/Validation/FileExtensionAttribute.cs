#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Framework_Project.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Kiểm tra xem giá trị có phải là IFormFile hay không.
            if(value is IFormFile file)
            {
                // Lấy phần mở rộng của tệp.
                var extension = Path.GetExtension(file.FileName);
                // Định nghĩa các phần mở rộng tệp được phép.
                string[] extensions = { ".jpg", ".png", ".jpeg" };

                // Kiểm tra xem phần mở rộng của tệp có nằm trong danh sách cho phép không.
                bool result = extensions.Any(x => extension.EndsWith(x, StringComparison.OrdinalIgnoreCase)); // Sử dụng StringComparison.OrdinalIgnoreCase để so sánh không phân biệt chữ hoa chữ thường.

                if(!result)
                {
                    return new ValidationResult("Loại file không được hỗ trợ. Vui lòng tải lên file .jpg, .png, hoặc .jpeg");
                }
                 
            }

            return ValidationResult.Success;
        }
    }
}
