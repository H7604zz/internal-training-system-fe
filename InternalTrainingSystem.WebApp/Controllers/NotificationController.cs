using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(ILogger<NotificationController> logger)
        {
            _logger = logger;
        }

        // View để hiển thị trang data sample
        public IActionResult DataSample()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(int page = 1, int pageSize = 10)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                // Tạo dữ liệu mẫu cho demo
                var sampleNotifications = GetSampleNotifications();
                
                var result = new NotificationListResponse
                {
                    Success = true,
                    Message = "Lấy thông báo thành công",
                    Notifications = sampleNotifications.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                    UnreadCount = sampleNotifications.Count(n => !n.IsRead),
                    TotalCount = sampleNotifications.Count
                };

                return Json(new 
                { 
                    success = result.Success, 
                    message = result.Message,
                    notifications = result.Notifications,
                    unreadCount = result.UnreadCount,
                    totalCount = result.TotalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách thông báo" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, count = 0 });
                }

                var sampleNotifications = GetSampleNotifications();
                var count = sampleNotifications.Count(n => !n.IsRead);
                
                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số thông báo chưa đọc");
                return Json(new { success = false, count = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                // Trong thực tế, sẽ call API để mark as read
                // Hiện tại chỉ return success cho demo
                return Json(new { success = true, message = "Đã đánh dấu thông báo đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                // Trong thực tế, sẽ call API để mark all as read
                // Hiện tại chỉ return success cho demo
                return Json(new { success = true, message = "Đã đánh dấu tất cả thông báo đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo đã đọc");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        // Demo endpoint để tạo thông báo mới
        [HttpPost]
        public IActionResult CreateSampleNotification(string type = "reminder")
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                var notificationType = type.ToLower() switch
                {
                    "start" => NotificationType.Start,
                    "finish" => NotificationType.Finish,
                    "certificate" => NotificationType.Certificate,
                    "staffconfirm" => NotificationType.StaffConfirm,
                    _ => NotificationType.Reminder
                };

                var sampleMessages = GetSampleMessagesByType(notificationType);
                var random = new Random();
                var randomMessage = sampleMessages[random.Next(sampleMessages.Count)];

                var newNotification = new NotificationDto
                {
                    Id = random.Next(1000, 9999),
                    Type = notificationType,
                    Message = randomMessage.Message,
                    CreatedAt = DateTime.Now,
                    CourseId = random.Next(1, 20),
                    IsRead = false,
                    Link = randomMessage.Link
                };

                return Json(new 
                { 
                    success = true, 
                    message = "Tạo thông báo mới thành công",
                    notification = newNotification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thông báo mới");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        private List<(string Message, string Link)> GetSampleMessagesByType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Start => new List<(string, string)>
                {
                    ("Khóa học 'React Native Mobile Development' sẽ bắt đầu vào thứ Hai tới lúc 9:00 AM", "/KhoaHoc/ChiTiet/1"),
                    ("Khóa học 'AWS Cloud Fundamentals' đã chính thức mở lớp", "/KhoaHoc/ChiTiet/2"),
                    ("Buổi học đầu tiên của 'Spring Boot Framework' sẽ diễn ra tomorrow", "/KhoaHoc/ChiTiet/3"),
                    ("Khóa học 'Flutter Development' bắt đầu với buổi orientation", "/KhoaHoc/ChiTiet/4")
                },
                NotificationType.Reminder => new List<(string, string)>
                {
                    ("Nhắc nhở: Bài tập 'RESTful API Design' cần nộp trước 5:00 PM hôm nay", "/KhoaHoc/ChiTiet/1"),
                    ("Đừng quên tham gia buổi review code vào 3:00 PM", "/LopHoc/ChiTiet/2"),
                    ("Bài kiểm tra 'MongoDB & NoSQL' sắp đến hạn (còn 1 ngày)", "/Quiz/DanhSachBaiQuiz"),
                    ("Reminder: Sprint planning meeting vào thứ Năm lúc 10:00 AM", "/LopHoc/ChiTiet/3"),
                    ("Hạn nộp project 'E-commerce Website' là ngày mai", "/KhoaHoc/ChiTiet/4")
                },
                NotificationType.Finish => new List<(string, string)>
                {
                    ("Xuất sắc! Bạn đã hoàn thành khóa học 'GraphQL & Apollo' với điểm 9.2/10", "/KhoaHoc/ChiTiet/1"),
                    ("Chúc mừng! Bạn đã pass tất cả test cases của project 'Blog System'", "/KhoaHoc/ChiTiet/2"),
                    ("Hoàn thành khóa học 'Redis & Caching Strategies' thành công", "/KhoaHoc/ChiTiet/3"),
                    ("Bạn đã hoàn tất khóa học 'Unit Testing với Jest' với kết quả tốt", "/KhoaHoc/ChiTiet/4")
                },
                NotificationType.Certificate => new List<(string, string)>
                {
                    ("Chứng chỉ 'Certified React Developer' đã được cấp và sẵn sàng tải xuống", "/KhoaHoc/DanhSachKhoaHocCuaToi"),
                    ("Chứng chỉ 'AWS Solution Architect Associate' đã được phê duyệt", "/KhoaHoc/DanhSachKhoaHocCuaToi"),
                    ("Chứng chỉ hoàn thành 'Advanced JavaScript Programming' đã sẵn sàng", "/KhoaHoc/DanhSachKhoaHocCuaToi"),
                    ("Certificate 'Full-Stack Developer' đã được issue cho profile của bạn", "/KhoaHoc/DanhSachKhoaHocCuaToi")
                },
                NotificationType.StaffConfirm => new List<(string, string)>
                {
                    ("Manager đã phê duyệt đăng ký khóa học 'Kubernetes Administration'", "/KhoaHoc/ChiTiet/1"),
                    ("Yêu cầu tham gia 'Advanced System Design' đã được chấp thuận", "/KhoaHoc/ChiTiet/2"),
                    ("Đăng ký khóa học 'Blockchain Development' cần xác nhận từ Team Lead", "/KhoaHoc/ChiTiet/3"),
                    ("HR đã approve việc tham gia conference 'Tech Summit 2025'", "/KhoaHoc/ChiTiet/4")
                },
                _ => new List<(string, string)>
                {
                    ("Thông báo chung: Hệ thống sẽ maintenance vào cuối tuần", "/"),
                    ("Cập nhật mới: Đã thêm tính năng chat trong lớp học", "/")
                }
            };
        }

        private List<NotificationDto> GetSampleNotifications()
        {
            return new List<NotificationDto>
            {
                // Thông báo chưa đọc
                new NotificationDto
                {
                    Id = 1,
                    Type = NotificationType.Start,
                    Message = "Khóa học 'Lập trình C# nâng cao' sẽ bắt đầu vào ngày mai lúc 9:00 AM. Hãy chuẩn bị tài liệu và tham gia đúng giờ.",
                    CreatedAt = DateTime.Now.AddMinutes(-15),
                    CourseId = 1,
                    IsRead = false,
                    Link = "/KhoaHoc/ChiTiet/1"
                },
                new NotificationDto
                {
                    Id = 2,
                    Type = NotificationType.Reminder,
                    Message = "Nhắc nhở: Bạn có bài kiểm tra 'ASP.NET Core MVC' sắp hết hạn trong 2 ngày. Vui lòng hoàn thành trước deadline.",
                    CreatedAt = DateTime.Now.AddHours(-1),
                    CourseId = 2,
                    IsRead = false,
                    Link = "/Quiz/DanhSachBaiQuiz"
                },
                new NotificationDto
                {
                    Id = 3,
                    Type = NotificationType.StaffConfirm,
                    Message = "Yêu cầu xác nhận tham gia khóa học 'Database Design & SQL Server' do Manager phê duyệt.",
                    CreatedAt = DateTime.Now.AddHours(-3),
                    CourseId = 5,
                    IsRead = false,
                    Link = "/KhoaHoc/ChiTiet/5"
                },
                new NotificationDto
                {
                    Id = 4,
                    Type = NotificationType.Reminder,
                    Message = "Lịch học hôm nay: Lớp 'React.js Fundamentals' bắt đầu lúc 14:00 tại phòng Lab 3.",
                    CreatedAt = DateTime.Now.AddHours(-4),
                    CourseId = 6,
                    IsRead = false,
                    Link = "/LopHoc/ChiTiet/6"
                },
                new NotificationDto
                {
                    Id = 5,
                    Type = NotificationType.Start,
                    Message = "Khóa học 'Git & Version Control' đã bắt đầu. Bạn có thể truy cập vào tài liệu và video bài giảng.",
                    CreatedAt = DateTime.Now.AddHours(-6),
                    CourseId = 7,
                    IsRead = false,
                    Link = "/KhoaHoc/ChiTiet/7"
                },

                // Thông báo đã đọc
                new NotificationDto
                {
                    Id = 6,
                    Type = NotificationType.Certificate,
                    Message = "Chứng chỉ cho khóa học 'JavaScript ES6+ và Modern Web Development' đã được cấp. Bạn có thể tải xuống từ trang cá nhân.",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    CourseId = 3,
                    IsRead = true,
                    Link = "/KhoaHoc/DanhSachKhoaHocCuaToi"
                },
                new NotificationDto
                {
                    Id = 7,
                    Type = NotificationType.Finish,
                    Message = "Chúc mừng! Bạn đã hoàn thành khóa học 'HTML5 & CSS3 Responsive Design' với điểm số 8.5/10.",
                    CreatedAt = DateTime.Now.AddDays(-1).AddHours(-3),
                    CourseId = 4,
                    IsRead = true,
                    Link = "/KhoaHoc/ChiTiet/4"
                },
                new NotificationDto
                {
                    Id = 8,
                    Type = NotificationType.Reminder,
                    Message = "Đã có bài tập mới trong khóa học 'Node.js & Express Framework'. Deadline nộp bài: 25/10/2025.",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    CourseId = 8,
                    IsRead = true,
                    Link = "/KhoaHoc/ChiTiet/8"
                },
                new NotificationDto
                {
                    Id = 9,
                    Type = NotificationType.StaffConfirm,
                    Message = "Bạn đã được chấp thuận tham gia khóa học 'DevOps với Docker & Kubernetes'. Khóa học bắt đầu 30/10/2025.",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    CourseId = 9,
                    IsRead = true,
                    Link = "/KhoaHoc/ChiTiet/9"
                },
                new NotificationDto
                {
                    Id = 10,
                    Type = NotificationType.Start,
                    Message = "Khóa học 'Python cho Data Science' đã mở đăng ký. Hạn chót đăng ký: 28/10/2025.",
                    CreatedAt = DateTime.Now.AddDays(-4),
                    CourseId = 10,
                    IsRead = true,
                    Link = "/KhoaHoc/ChiTiet/10"
                },
                new NotificationDto
                {
                    Id = 11,
                    Type = NotificationType.Finish,
                    Message = "Bạn đã hoàn thành tất cả bài kiểm tra trong khóa học 'Angular Framework'. Đang chờ review từ giảng viên.",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CourseId = 11,
                    IsRead = true,
                    Link = "/Quiz/KetQua/11"
                },
                new NotificationDto
                {
                    Id = 12,
                    Type = NotificationType.Certificate,
                    Message = "Chứng chỉ 'Vue.js Development' của bạn đã được phê duyệt và sẵn sàng để tải xuống.",
                    CreatedAt = DateTime.Now.AddDays(-6),
                    CourseId = 12,
                    IsRead = true,
                    Link = "/KhoaHoc/DanhSachKhoaHocCuaToi"
                },
                new NotificationDto
                {
                    Id = 13,
                    Type = NotificationType.Reminder,
                    Message = "Reminder: Team meeting về dự án thực tế 'E-commerce Website' vào thứ Hai lúc 10:00 AM.",
                    CreatedAt = DateTime.Now.AddDays(-7),
                    CourseId = 13,
                    IsRead = true,
                    Link = "/LopHoc/ChiTiet/13"
                },
                new NotificationDto
                {
                    Id = 14,
                    Type = NotificationType.StaffConfirm,
                    Message = "Yêu cầu tham gia khóa học 'Machine Learning with Python' đã được gửi đến Manager để phê duyệt.",
                    CreatedAt = DateTime.Now.AddDays(-8),
                    CourseId = 14,
                    IsRead = true,
                    Link = "/KhoaHoc/ChiTiet/14"
                },
                new NotificationDto
                {
                    Id = 15,
                    Type = NotificationType.Start,
                    Message = "Khóa học 'Microservices Architecture' sẽ có buổi orientation vào thứ Tư tuần tới.",
                    CreatedAt = DateTime.Now.AddDays(-9),
                    CourseId = 15,
                    IsRead = true,
                    Link = "/KhoaHoc/ChiTiet/15"
                }
            };
        }
    }
}