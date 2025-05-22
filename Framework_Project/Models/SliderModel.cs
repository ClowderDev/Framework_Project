using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Framework_Project.Repository.Validation;

namespace Framework_Project.Models
{
    public class SliderModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được bỏ trống tên slider")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được bỏ trống mô tả")]
		public string Description { get; set; }
		public int? Status { get; set; }

		public string Image { get; set; }

		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}