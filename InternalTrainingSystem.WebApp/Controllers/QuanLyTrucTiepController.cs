using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class QuanLyTrucTiepController : Controller
    {
        private readonly ILogger<QuanLyTrucTiepController> _logger;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public QuanLyTrucTiepController(
            ILogger<QuanLyTrucTiepController> logger,
            ICourseService courseService,
            IUserService userService,
            INotificationService notificationService)
        {
            _logger = logger;
            _courseService = courseService;
            _userService = userService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Trang chủ Dashboard của quản lý trực tiếp
        /// URL: /QuanLyTrucTiep/
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Trang chính của quản lý trực tiếp - Hiển thị danh sách khóa học sắp mở lớp
        /// URL: /QuanLyTrucTiep/danh-sach-khoa-hoc-sap-mo
        /// </summary>
        public async Task<IActionResult> DanhSachKhoaHocSapMo(int page = 1, int pageSize = 10, string search = "", string category = "", string level = "")
        {
            try
            {
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                ViewBag.Category = category;
                ViewBag.Level = level;

                // Mock data for demo - replace with actual service call
                var courses = GetMockUpcomingCourses(page, pageSize, search, category, level);
                
                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách khóa học sắp mở lớp");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách khóa học";
                return View(new List<CourseDto>());
            }
        }

        /// <summary>
        /// Trang hiển thị danh sách staff đủ điều kiện học khóa học
        /// URL: /QuanLyTrucTiep/danh-sach-staff-du-dieu-kien/{courseId}
        /// </summary>
        public async Task<IActionResult> DanhSachStaffDuDieuKien(int courseId, int page = 1, int pageSize = 10)
        {
            try
            {
                // Mock course data
                var course = GetMockCourse(courseId);
                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học";
                    return RedirectToAction("DanhSachKhoaHocSapMo");
                }

                // Mock notification status
                var isNotificationSent = courseId % 2 == 0; // Mock logic
                var sentAt = isNotificationSent ? DateTime.Now.AddHours(-2) : (DateTime?)null;

                ViewBag.CourseId = courseId;
                ViewBag.Course = course;
                ViewBag.IsNotificationSent = isNotificationSent;
                ViewBag.SentAt = sentAt;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                // Mock eligible staff data
                var eligibleStaff = GetMockEligibleStaff(courseId, page, pageSize);
                
                return View(eligibleStaff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách staff đủ điều kiện cho khóa học {CourseId}", courseId);
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách nhân viên";
                return RedirectToAction("DanhSachKhoaHocSapMo");
            }
        }

        /// <summary>
        /// Mời staff tham gia khóa học
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MoiStaffThamGia(int courseId, int staffId)
        {
            try
            {
                // Mock successful invitation
                TempData["SuccessMessage"] = "Đã gửi lời mời thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi mời staff {StaffId} tham gia khóa học {CourseId}", staffId, courseId);
                TempData["Error"] = "Có lỗi xảy ra khi gửi lời mời";
            }

            return RedirectToAction("DanhSachStaffDuDieuKien", new { courseId });
        }

        /// <summary>
        /// Gửi thông báo cho tất cả staff đủ điều kiện
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GuiThongBaoTatCaStaff(int courseId)
        {
            try
            {
                // Mock successful notification
                TempData["SuccessMessage"] = "Đã gửi thông báo cho tất cả nhân viên đủ điều kiện";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo cho tất cả staff của khóa học {CourseId}", courseId);
                TempData["Error"] = "Có lỗi xảy ra khi gửi thông báo";
            }

            return RedirectToAction("DanhSachStaffDuDieuKien", new { courseId });
        }

        /// <summary>
        /// API lấy thống kê tổng quan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetThongKeTongQuan()
        {
            try
            {
                var stats = new DirectManagerStats
                {
                    TotalCourses = 8,
                    PendingStaff = 25,
                    InvitedStaff = 15,
                    ConfirmedStaff = 10
                };
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê tổng quan");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê" });
            }
        }

        // Mock data methods
        private List<CourseDto> GetMockUpcomingCourses(int page, int pageSize, string search, string category, string level)
        {
            var allCourses = new List<CourseDto>
            {
                new CourseDto { Id = 1, CourseName = "C# Programming Fundamentals", Code = "CS001", Description = "Học lập trình C# từ cơ bản đến nâng cao", Category = "lap-trinh", Level = "co-ban", StartDate = DateTime.Now.AddDays(10) },
                new CourseDto { Id = 2, CourseName = "Project Management Essentials", Code = "PM001", Description = "Kỹ năng quản lý dự án hiệu quả", Category = "quan-ly", Level = "trung-binh", StartDate = DateTime.Now.AddDays(15) },
                new CourseDto { Id = 3, CourseName = "Digital Marketing Strategy", Code = "MK001", Description = "Chiến lược marketing kỹ thuật số", Category = "ky-nang-mem", Level = "nang-cao", StartDate = DateTime.Now.AddDays(20) },
                new CourseDto { Id = 4, CourseName = "Leadership Development", Code = "LD001", Description = "Phát triển kỹ năng lãnh đạo", Category = "quan-ly", Level = "nang-cao", StartDate = DateTime.Now.AddDays(25) },
                new CourseDto { Id = 5, CourseName = "Data Analytics with Python", Code = "DA001", Description = "Phân tích dữ liệu với Python", Category = "cong-nghe", Level = "trung-binh", StartDate = DateTime.Now.AddDays(30) },
                new CourseDto { Id = 6, CourseName = "Business English Communication", Code = "EN001", Description = "Giao tiếp tiếng Anh trong kinh doanh", Category = "ngoai-ngu", Level = "co-ban", StartDate = DateTime.Now.AddDays(35) },
                new CourseDto { Id = 7, CourseName = "Advanced SQL Database", Code = "DB001", Description = "Quản lý cơ sở dữ liệu SQL nâng cao", Category = "cong-nghe", Level = "nang-cao", StartDate = DateTime.Now.AddDays(40) },
                new CourseDto { Id = 8, CourseName = "Agile Software Development", Code = "AG001", Description = "Phương pháp phát triển phần mềm Agile", Category = "lap-trinh", Level = "trung-binh", StartDate = DateTime.Now.AddDays(45) }
            };

            // Filter by search
            if (!string.IsNullOrEmpty(search))
            {
                allCourses = allCourses.Where(c => c.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                                   (c.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                allCourses = allCourses.Where(c => c.Category == category).ToList();
            }

            // Filter by level
            if (!string.IsNullOrEmpty(level))
            {
                allCourses = allCourses.Where(c => c.Level == level).ToList();
            }

            // Pagination
            return allCourses.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        private CourseDto? GetMockCourse(int courseId)
        {
            var courses = GetMockUpcomingCourses(1, 100, "", "", "");
            return courses.FirstOrDefault(c => c.Id == courseId);
        }

        private List<UserDto> GetMockEligibleStaff(int courseId, int page, int pageSize)
        {
            var allStaff = new List<UserDto>
            {
                new UserDto { Id = 1, FullName = "Nguyễn Văn An", Email = "an.nguyen@company.com", Department = "IT", Position = "Developer", YearsOfExperience = 3, IsInvited = true, HasConfirmed = true },
                new UserDto { Id = 2, FullName = "Trần Thị Bình", Email = "binh.tran@company.com", Department = "Marketing", Position = "Marketing Executive", YearsOfExperience = 2, IsInvited = true, HasConfirmed = false },
                new UserDto { Id = 3, FullName = "Lê Văn Cường", Email = "cuong.le@company.com", Department = "HR", Position = "HR Specialist", YearsOfExperience = 4, IsInvited = false, HasConfirmed = false },
                new UserDto { Id = 4, FullName = "Phạm Thị Dung", Email = "dung.pham@company.com", Department = "Finance", Position = "Accountant", YearsOfExperience = 5, IsInvited = false, HasConfirmed = false },
                new UserDto { Id = 5, FullName = "Hoàng Văn Em", Email = "em.hoang@company.com", Department = "IT", Position = "Senior Developer", YearsOfExperience = 6, IsInvited = true, HasConfirmed = true },
                new UserDto { Id = 6, FullName = "Vũ Thị Phương", Email = "phuong.vu@company.com", Department = "Sales", Position = "Sales Manager", YearsOfExperience = 7, IsInvited = false, HasConfirmed = false },
                new UserDto { Id = 7, FullName = "Đỗ Văn Giang", Email = "giang.do@company.com", Department = "IT", Position = "Team Lead", YearsOfExperience = 8, IsInvited = true, HasConfirmed = false },
                new UserDto { Id = 8, FullName = "Ngô Thị Hạnh", Email = "hanh.ngo@company.com", Department = "Customer Service", Position = "CS Representative", YearsOfExperience = 2, IsInvited = false, HasConfirmed = false }
            };

            return allStaff.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}