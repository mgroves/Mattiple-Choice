namespace MattipleChoice.Models
{
    public class Answer
    {
        public int? AnswerId { get; set; }
        public int? QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
}