using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Models.Quiz;
using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class QuizController : Controller
    {
        public IActionResult DanhSachBaiQuiz(string search = "", int page = 1)
        {
            var pageSize = PaginationConstants.QuizPageSize;
            
            // Lấy danh sách quiz mẫu với rich data
            var allQuizzes = GetSampleQuizzes();
            
            // Tìm kiếm theo title, description, subject, createdBy
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                allQuizzes = allQuizzes.Where(q => 
                    q.Title.ToLower().Contains(searchLower) ||
                    q.Description.ToLower().Contains(searchLower) ||
                    q.Subject.ToLower().Contains(searchLower) ||
                    q.CreatedBy.ToLower().Contains(searchLower)
                ).ToList();
            }
            
            // Phân trang
            var totalItems = allQuizzes.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedQuizzes = allQuizzes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // ViewBag cho pagination và search
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentSearch = search;
            
            // ViewBag cho thống kê
            ViewBag.TotalQuizzes = GetSampleQuizzes().Count; // Tổng không filter
            ViewBag.FilteredCount = allQuizzes.Count; // Số lượng sau khi search
            ViewBag.CompletedCount = allQuizzes.Count(q => q.IsCompleted);
            ViewBag.AvailableCount = allQuizzes.Count(q => q.Status == "Available");
            
            return View(pagedQuizzes);
        }

        private List<QuizViewModel> GetSampleQuizzes()
        {
            return new List<QuizViewModel>
            {
                new QuizViewModel
                {
                    Id = 1,
                    Title = "Kiểm tra kiến thức C# cơ bản",
                    Description = "Bài kiểm tra đánh giá kiến thức cơ bản về ngôn ngữ lập trình C#",
                    Subject = "Lập trình",
                    TimeLimit = 30,
                    QuestionCount = 15,
                    CreatedDate = DateTime.Now.AddDays(-10),
                    CreatedBy = "Nguyễn Văn A",
                    AttemptCount = 3,
                    BestScore = 85.5,
                    LastAttempt = DateTime.Now.AddDays(-2),
                    IsCompleted = true,
                    Status = "Completed"
                },
                new QuizViewModel
                {
                    Id = 2,
                    Title = "SQL Server và T-SQL nâng cao",
                    Description = "Kiểm tra khả năng viết query phức tạp và tối ưu hóa database",
                    Subject = "Cơ sở dữ liệu",
                    TimeLimit = 90,
                    QuestionCount = 25,
                    CreatedDate = DateTime.Now.AddDays(-15),
                    CreatedBy = "Trần Thị B",
                    AttemptCount = 1,
                    BestScore = 72.0,
                    LastAttempt = DateTime.Now.AddDays(-5),
                    IsCompleted = true,
                    Status = "Completed"
                },
                new QuizViewModel
                {
                    Id = 3,
                    Title = "Cấu hình mạng và troubleshooting",
                    Description = "Bài test về cấu hình router, switch và xử lý sự cố mạng",
                    Subject = "Mạng máy tính",
                    TimeLimit = 60,
                    QuestionCount = 20,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    CreatedBy = "Phạm Văn C",
                    AttemptCount = 0,
                    BestScore = null,
                    LastAttempt = null,
                    IsCompleted = false,
                    Status = "Available"
                },
                new QuizViewModel
                {
                    Id = 4,
                    Title = "Cybersecurity và bảo mật thông tin",
                    Description = "Đánh giá kiến thức về bảo mật hệ thống và phòng chống tấn công",
                    Subject = "Bảo mật",
                    TimeLimit = 75,
                    QuestionCount = 30,
                    CreatedDate = DateTime.Now.AddDays(-20),
                    CreatedBy = "Lê Thị D",
                    AttemptCount = 2,
                    BestScore = 78.5,
                    LastAttempt = DateTime.Now.AddDays(-1),
                    IsCompleted = true,
                    Status = "Completed"
                },
                new QuizViewModel
                {
                    Id = 5,
                    Title = "UI/UX Design principles",
                    Description = "Kiểm tra hiểu biết về nguyên tắc thiết kế giao diện người dùng",
                    Subject = "Thiết kế",
                    TimeLimit = 45,
                    QuestionCount = 18,
                    CreatedDate = DateTime.Now.AddDays(-8),
                    CreatedBy = "Hoàng Văn E",
                    AttemptCount = 1,
                    BestScore = 90.0,
                    LastAttempt = DateTime.Now.AddDays(-3),
                    IsCompleted = true,
                    Status = "Completed"
                },
                new QuizViewModel
                {
                    Id = 6,
                    Title = "Agile & Scrum Framework",
                    Description = "Bài test về phương pháp quản lý dự án Agile và Scrum",
                    Subject = "Quản lý dự án",
                    TimeLimit = 40,
                    QuestionCount = 22,
                    CreatedDate = DateTime.Now.AddDays(-12),
                    CreatedBy = "Võ Thị F",
                    AttemptCount = 0,
                    BestScore = null,
                    LastAttempt = null,
                    IsCompleted = false,
                    Status = "Available"
                },
                new QuizViewModel
                {
                    Id = 7,
                    Title = "Machine Learning cơ bản",
                    Description = "Kiến thức nền tảng về học máy và các thuật toán cơ bản",
                    Subject = "AI/ML",
                    TimeLimit = 120,
                    QuestionCount = 35,
                    CreatedDate = DateTime.Now.AddDays(-25),
                    CreatedBy = "Đặng Văn G",
                    AttemptCount = 0,
                    BestScore = null,
                    LastAttempt = null,
                    IsCompleted = false,
                    Status = "Available"
                },
                new QuizViewModel
                {
                    Id = 8,
                    Title = "Docker và Kubernetes",
                    Description = "Kiểm tra kiến thức về containerization và orchestration",
                    Subject = "DevOps",
                    TimeLimit = 90,
                    QuestionCount = 28,
                    CreatedDate = DateTime.Now.AddDays(-6),
                    CreatedBy = "Ngô Thị H",
                    AttemptCount = 1,
                    BestScore = 65.0,
                    LastAttempt = DateTime.Now.AddDays(-4),
                    IsCompleted = false,
                    Status = "In-Progress"
                },
                new QuizViewModel
                {
                    Id = 9,
                    Title = "React Native Development",
                    Description = "Phát triển ứng dụng mobile với React Native framework",
                    Subject = "Mobile",
                    TimeLimit = 70,
                    QuestionCount = 24,
                    CreatedDate = DateTime.Now.AddDays(-18),
                    CreatedBy = "Bùi Văn I",
                    AttemptCount = 2,
                    BestScore = 88.0,
                    LastAttempt = DateTime.Now.AddDays(-7),
                    IsCompleted = true,
                    Status = "Completed"
                },
                new QuizViewModel
                {
                    Id = 10,
                    Title = "Modern Web Development",
                    Description = "Công nghệ web hiện đại: HTML5, CSS3, JavaScript ES6+",
                    Subject = "Web Development",
                    TimeLimit = 55,
                    QuestionCount = 20,
                    CreatedDate = DateTime.Now.AddDays(-3),
                    CreatedBy = "Lý Thị K",
                    AttemptCount = 0,
                    BestScore = null,
                    LastAttempt = null,
                    IsCompleted = false,
                    Status = "Available"
                }
            };
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
                return RedirectToAction("DanhSachBaiQuiz");
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
                TimeLimit = 60, // 60 phút
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
            return RedirectToAction("KetQua", new { id = submission.QuizId });
        }

        public IActionResult KetQua(int id)
        {
            // Hiển thị kết quả
            return View();
        }
    }
}