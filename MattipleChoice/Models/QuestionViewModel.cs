using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MattipleChoice.Models
{
    public class QuestionViewModel
    {
        [Required]
        public string QuestionText { get; set; }
        public List<Answer> Answers { get; set; }
    }
}