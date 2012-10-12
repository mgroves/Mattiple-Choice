namespace MattipleChoice.Models
{
    public class Question
    {
        public int? QuestionId { get; set; }
        public int? ExamId { get; set; }
        public string QuestionText { get; set; }
    }
}