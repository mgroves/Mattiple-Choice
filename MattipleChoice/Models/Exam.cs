using System.ComponentModel.DataAnnotations;

namespace MattipleChoice.Models
{
    public class Exam
    {
        public int? ExamId { get; set;}

        [Required]
        public string ExamName { get; set; }
    }
}