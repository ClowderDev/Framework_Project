using System.ComponentModel.DataAnnotations;

namespace Framework_Project.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")] 
        public string Description { get; set; }

        public string Slug { get; set; }

        public int Status { get; set; }
    }
}
