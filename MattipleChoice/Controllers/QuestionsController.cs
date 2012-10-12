using System.Data.SqlClient;
using System.Web.Mvc;
using Dapper;
using MattipleChoice.Models;
using System.Linq;

namespace MattipleChoice.Controllers
{
    public class QuestionsController : Controller
    {
        readonly SqlConnection _connection;

        public QuestionsController()
        {
            _connection = new SqlConnection("server=(local);uid=;pwd=;Trusted_Connection=yes;database=mus215;");
        }

        public ViewResult Index(int? id)
        {
            _connection.Open();
            var questions = _connection.Query<Question>("SELECT QuestionId, ExamId, QuestionText FROM Questions WHERE ExamId = @ExamId", new { ExamId = id });
            _connection.Close();
            if (id.HasValue)
                ViewBag.ExamId = id.Value;
            return View(questions);
        }

        public ViewResult Create(int? id)
        {
            ViewBag.ExamId = id;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(int? id, QuestionViewModel questionViewModel)
        {
            ViewBag.ExamId = id;
            var moreThanOneAnswer = questionViewModel.Answers.Count(a => a.IsCorrect) != 1;
            if(moreThanOneAnswer)
                ModelState.AddModelError("OnlyOneAnswer","There must be one and only one correct answer");
            var anyBlankAnswers = questionViewModel.Answers.Any(a => string.IsNullOrEmpty(a.AnswerText));
            if(anyBlankAnswers)
                ModelState.AddModelError("BlankAnswers", "You must enter text for all answers");

            if (!ModelState.IsValid)
                return View(questionViewModel);

            _connection.Open();
            var questionId = _connection.Query<int>("INSERT INTO Questions (ExamId, QuestionText) VALUES (@ExamId, @QuestionText); SELECT CAST(SCOPE_IDENTITY() AS int);", 
                new { ExamId = id, questionViewModel.QuestionText });
            for (var i = 0; i < 4;i++)
            {
                var answer = questionViewModel.Answers[i];
                _connection.Execute("INSERT INTO Answers (AnswerText, QuestionId, IsCorrect) VALUES (@AnswerText, @QuestionId, @IsCorrect)",
                    new { answer.AnswerText, QuestionId = questionId, answer.IsCorrect });
            }
            _connection.Close();
            return RedirectToAction("Index", new { id });
        }

        public ActionResult Edit(int? id)
        {
            _connection.Open();
            var questionViewModel = new QuestionViewModel();
            var question = _connection.Query<Question>("SELECT TOP 1 QuestionText, ExamId FROM Questions WHERE QuestionId = @QuestionId", new { QuestionId = id }).Single();
            ViewBag.ExamId = question.ExamId;
            questionViewModel.QuestionText = question.QuestionText;
            questionViewModel.Answers = _connection.Query<Answer>("SELECT AnswerId, AnswerText, IsCorrect FROM Answers WHERE QuestionId = @QuestionId", new { QuestionId = id }).ToList();
            _connection.Close();
            return View(questionViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int? id, QuestionViewModel questionViewModel)
        {
            _connection.Open();
            var examId = _connection.Query<int>("SELECT TOP 1 ExamId FROM Questions WHERE QuestionId = @QuestionId", new { QuestionId = id }).Single();
            ViewBag.ExamId = examId;

            var moreThanOneAnswer = questionViewModel.Answers.Count(a => a.IsCorrect) != 1;
            if(moreThanOneAnswer)
                ModelState.AddModelError("OnlyOneAnswer","There must be one and only one correct answer");
            var anyBlankAnswers = questionViewModel.Answers.Any(a => string.IsNullOrEmpty(a.AnswerText));
            if(anyBlankAnswers)
                ModelState.AddModelError("BlankAnswers", "You must enter text for all answers");

            if (!ModelState.IsValid)
            {
                _connection.Close();
                return View(questionViewModel);
            }

            _connection.Execute("UPDATE Questions SET QuestionText = @QuestionText WHERE QuestionId = @QuestionId",
                                new {QuestionId = id, questionViewModel.QuestionText});
            foreach (var answer in questionViewModel.Answers)
            {
                _connection.Execute("UPDATE Answers SET AnswerText = @AnswerText, IsCorrect = @IsCorrect WHERE AnswerId = @AnswerId",
                    new { answer.AnswerText, answer.IsCorrect, answer.AnswerId });
            }
            _connection.Close();
            return RedirectToAction("Index", new { id = examId });
        }
    }
}