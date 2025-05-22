using System.ComponentModel.DataAnnotations;

namespace Framework_Project.Models
{
    public class StatisticalModel
    {
        [Key]
        public int Id { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }

    }
}
