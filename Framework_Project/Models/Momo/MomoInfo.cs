using System.ComponentModel.DataAnnotations;

namespace Framework_Project.Models.Momo
{
    public class MomoInfo
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public DateTime DatePaid { get; set; }
    }
}
