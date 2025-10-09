using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Models.Quiz;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class QuizController : Controller
    {
        public IActionResult DanhSachBaiQuiz()
        {
            return View();
        }

        // Trang tạo quiz cho quản lý
        public IActionResult TaoQuiz()
        {
            var model = new CreateQuizViewModel
            {
                Chapters = GetSampleChapters(),
                Sessions = new List<SessionViewModel>()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult TaoQuiz(CreateQuizViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Xử lý tạo quiz
                // TODO: Lưu vào database
                
                TempData["SuccessMessage"] = "Tạo bài kiểm tra thành công!";
                return RedirectToAction("Index");
            }

            // Nếu có lỗi, load lại data
            model.Chapters = GetSampleChapters();
            model.Sessions = GetSessionsByChapterPrivate(model.SelectedChapterId);
            return View(model);
        }

        // API để lấy sessions theo chapter
        [HttpGet]
        public JsonResult GetSessionsByChapter(int chapterId)
        {
            var sessions = GetSessionsByChapterPrivate(chapterId);
            return Json(sessions);
        }

        // Helper method để lấy sample data
        private List<ChapterViewModel> GetSampleChapters()
        {
            return new List<ChapterViewModel>
            {
                new ChapterViewModel
                {
                    Id = 1,
                    Name = "Chương 1: Cơ bản về C#",
                    Description = "Học về cú pháp cơ bản và kiểu dữ liệu",
                    Sessions = GetSessionsByChapterPrivate(1)
                },
                new ChapterViewModel
                {
                    Id = 2,
                    Name = "Chương 2: OOP trong C#",
                    Description = "Lập trình hướng đối tượng",
                    Sessions = GetSessionsByChapterPrivate(2)
                },
                new ChapterViewModel
                {
                    Id = 3,
                    Name = "Chương 3: Collections và LINQ",
                    Description = "Làm việc với collections và LINQ",
                    Sessions = GetSessionsByChapterPrivate(3)
                }
            };
        }

        private List<SessionViewModel> GetSessionsByChapterPrivate(int chapterId)
        {
            switch (chapterId)
            {
                case 1:
                    return new List<SessionViewModel>
                    {
                        new SessionViewModel { Id = 1, Name = "Session 1.1: Biến và kiểu dữ liệu", ChapterId = 1, QuestionCount = 15 },
                        new SessionViewModel { Id = 2, Name = "Session 1.2: Toán tử và biểu thức", ChapterId = 1, QuestionCount = 12 },
                        new SessionViewModel { Id = 3, Name = "Session 1.3: Cấu trúc điều khiển", ChapterId = 1, QuestionCount = 18 }
                    };
                case 2:
                    return new List<SessionViewModel>
                    {
                        new SessionViewModel { Id = 4, Name = "Session 2.1: Class và Object", ChapterId = 2, QuestionCount = 20 },
                        new SessionViewModel { Id = 5, Name = "Session 2.2: Inheritance", ChapterId = 2, QuestionCount = 16 },
                        new SessionViewModel { Id = 6, Name = "Session 2.3: Polymorphism", ChapterId = 2, QuestionCount = 14 }
                    };
                case 3:
                    return new List<SessionViewModel>
                    {
                        new SessionViewModel { Id = 7, Name = "Session 3.1: List và Array", ChapterId = 3, QuestionCount = 22 },
                        new SessionViewModel { Id = 8, Name = "Session 3.2: Dictionary và HashSet", ChapterId = 3, QuestionCount = 18 },
                        new SessionViewModel { Id = 9, Name = "Session 3.3: LINQ Basics", ChapterId = 3, QuestionCount = 25 }
                    };
                default:
                    return new List<SessionViewModel>();
            }
        }

        public IActionResult LamQuiz(int id = 1)
        {
            // Giả lập dữ liệu quiz
            var quiz = new QuizViewModel
            {
                Id = id,
                Title = "Bài Kiểm Tra Lập Trình C#",
                Duration = 60, // 60 phút
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Id = 1,
                        Content = "Từ khóa nào được sử dụng để khai báo một lớp trong C#?",
                        Options = new List<string> { "class", "struct", "interface", "enum" },
                        CorrectAnswer = 0
                    },
                    new QuestionViewModel
                    {
                        Id = 2,
                        Content = "Phương thức nào được gọi khi một đối tượng được tạo?",
                        Options = new List<string> { "Destructor", "Constructor", "Finalizer", "Initializer" },
                        CorrectAnswer = 1
                    },
                    new QuestionViewModel
                    {
                        Id = 3,
                        Content = "Từ khóa 'virtual' được sử dụng để làm gì?",
                        Options = new List<string> { "Tạo biến ảo", "Cho phép override method", "Tạo class trừu tượng", "Khai báo interface" },
                        CorrectAnswer = 1
                    },
                    new QuestionViewModel
                    {
                        Id = 4,
                        Content = "Kiểu dữ liệu nào lưu trữ giá trị true/false?",
                        Options = new List<string> { "int", "string", "bool", "char" },
                        CorrectAnswer = 2
                    },
                    new QuestionViewModel
                    {
                        Id = 5,
                        Content = "Từ khóa 'static' có ý nghĩa gì?",
                        Options = new List<string> { "Thuộc về class chứ không phải instance", "Không thể thay đổi", "Riêng tư", "Công khai" },
                        CorrectAnswer = 0
                    }
                }
            };

            return View(quiz);
        }

        [HttpPost]
        public IActionResult SubmitQuiz(QuizSubmissionModel submission)
        {
            // Xử lý nộp bài
            return RedirectToAction("Result", new { id = submission.QuizId });
        }

        public IActionResult KetQua(int id)
        {
            // Hiển thị kết quả
            return View();
        }
    }
}