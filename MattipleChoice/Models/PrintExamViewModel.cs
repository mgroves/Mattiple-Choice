using System;
using System.Collections.Generic;

namespace MattipleChoice.Models
{
    public class PrintExamViewModel
    {
        public PrintExamViewModel()
        {
            AnswerKey = new List<string>();
            Questions = new List<QuestionViewModel>();
        }

        public Exam Exam { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
        public List<string> AnswerKey { get; set; }
        public Guid ExamKey { get; set; }
    }
}