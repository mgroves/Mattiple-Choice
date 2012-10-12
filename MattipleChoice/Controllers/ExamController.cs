using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using MattipleChoice.Models;

namespace MattipleChoice.Controllers
{
    public class ExamController : Controller
    {
        readonly SqlConnection _connection;

        public ExamController()
        {
            _connection = new SqlConnection("server=(local);uid=;pwd=;Trusted_Connection=yes;database=mus215;");
        }

        public ViewResult Index()
        {
            _connection.Open();
            var exams = _connection.Query<Exam>("SELECT ExamId, ExamName FROM Exams");
            _connection.Close();
            return View(exams);
        }

        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                _connection.Open();
                _connection.Execute("INSERT INTO Exams (ExamName) VALUES (@ExamName)", exam);
                _connection.Close();
                return RedirectToAction("Index");
            }
            return View(exam);
        }

        public ActionResult Delete(int? id)
        {
            _connection.Open();
            string httpMethod = Request.HttpMethod.ToUpperInvariant();
            if (httpMethod == "POST")
            {
                _connection.Execute("DELETE FROM Exams WHERE ExamId = @ExamId", new { ExamId = id });
                _connection.Close();
                return RedirectToAction("Index");
            }
            var exam = _connection.Query<Exam>("SELECT TOP 1 ExamId, ExamName FROM Exams WHERE ExamId = @ExamId", new {ExamId = id});
            _connection.Close();
            if (exam.Any())
                return View(exam.Single());
            return RedirectToAction("Index");
        }

        public ViewResult Edit(int? id)
        {
            _connection.Open();
            var exam = _connection.Query<Exam>("SELECT ExamId, ExamName FROM Exams WHERE ExamId = @ExamId", new { ExamId = id }).Single();
            _connection.Close();
            return View(exam);
        }

        [HttpPost]
        public ActionResult Edit(int? id, Exam exam)
        {
            if (!ModelState.IsValid)
                return View(exam);
            _connection.Open();
            _connection.Execute("UPDATE Exams SET ExamName = @ExamName WHERE ExamId = @ExamId", new { ExamName = exam.ExamName, ExamId = id });
            _connection.Close();
            return RedirectToAction("Index");
        }


        public ViewResult Print(int? id)
        {
            _connection.Open();
            var printExamViewModel = new PrintExamViewModel();
            printExamViewModel.ExamKey = Guid.NewGuid();
            printExamViewModel.Exam = _connection.Query<Exam>("SELECT TOP 1 ExamId, ExamName FROM Exams WHERE ExamId = @ExamId", new { ExamId = id }).Single();

            var questions = _connection.Query<Question>("SELECT QuestionId, QuestionText FROM Questions WHERE ExamId = @ExamId ORDER BY NEWID()", new { ExamId = id });
            var questionNumber = 1;
            foreach (var question in questions)
            {
                var questionViewModel = new QuestionViewModel();
                questionViewModel.QuestionText = question.QuestionText;
                questionViewModel.Answers = _connection.Query<Answer>("SELECT AnswerId, AnswerText, IsCorrect FROM Answers WHERE QuestionId = @QuestionID ORDER BY NEWID()",
                    new {question.QuestionId }).ToList();
                var correctAnswer = questionViewModel.Answers.Single(a => a.IsCorrect);
                printExamViewModel.AnswerKey.Add(ConvertNumToLetter(questionViewModel.Answers.IndexOf(correctAnswer)));
                printExamViewModel.Questions.Add(questionViewModel);
                questionNumber++;
            }
            _connection.Close();
            return View(printExamViewModel);
        }

        string ConvertNumToLetter(int index)
        {
            if (index == 0) return "a";
            if (index == 1) return "b";
            if (index == 2) return "c";
            return "d";
        }
    }
}