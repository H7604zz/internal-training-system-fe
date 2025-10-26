using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Models.ViewModels;
using InternalTrainingSystem.WebApp.Services.Interface;
using InternalTrainingSystem.WebApp.Constants;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("lop-hoc")]
    public class LopHocController : Controller
    {
        private readonly IClassService _classService;
        private readonly ILogger<LopHocController> _logger;

        public LopHocController(IClassService classService, ILogger<LopHocController> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                // Sử dụng page size cố định từ constants cho danh sách lớp học
                var pageSize = PaginationConstants.ClassPageSize;
                
                var allClasses = await _classService.GetClassesAsync();
               // var allClasses = GetSampleClassData(); // Tạo data mẫu để test view
                
                var totalItems = allClasses.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                
                var pagedClasses = allClasses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;

                return View(pagedClasses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting classes list");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách lớp học.";
                return View(new List<ClassDto>());
            }
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                // var allClasses = await _classService.GetClassesAsync();
                var allClasses = GetSampleClassData(); // Sử dụng data mẫu để test
                var classDetail = allClasses.FirstOrDefault(c => c.ClassId == id);

                if (classDetail == null)
                {
                    TempData["Error"] = "Không tìm thấy lớp học.";
                    return RedirectToAction("Index");
                }

                return View(classDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class detail with ID: {ClassId}", id);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết lớp học.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("tao-lop")]
        public IActionResult TaoLop()
        {
            try
            {
                var model = new CreateClassViewModel
                {
                    Courses = GetSampleCourses(),
                    Mentors = GetSampleMentors(),
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(3),
                    Capacity = 20,
                    Schedule = new List<ClassScheduleItem>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create class page");
                TempData["Error"] = "Đã xảy ra lỗi khi tải trang tạo lớp học.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("tao-lop")]
        public async Task<IActionResult> TaoLop(CreateClassViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Courses = GetSampleCourses();
                    model.Mentors = GetSampleMentors();
                    return View(model);
                }

                // Validate dates
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
                    model.Courses = GetSampleCourses();
                    model.Mentors = GetSampleMentors();
                    return View(model);
                }

                // TODO: Implement actual class creation logic
                // await _classService.CreateClassAsync(model);

                TempData["Success"] = "Tạo lớp học thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating class");
                TempData["Error"] = "Đã xảy ra lỗi khi tạo lớp học.";
                model.Courses = GetSampleCourses();
                model.Mentors = GetSampleMentors();
                return View(model);
            }
        }

        private List<CourseDto> GetSampleCourses()
        {
            return new List<CourseDto>
            {
                new CourseDto { CourseId = 101, CourseName = "Lập Trình C# Cơ Bản", Level = "Beginner" },
                new CourseDto { CourseId = 102, CourseName = "ASP.NET Core Web API Development", Level = "Intermediate" },
                new CourseDto { CourseId = 103, CourseName = "React JS Frontend Development", Level = "Intermediate" },
                new CourseDto { CourseId = 104, CourseName = "SQL Server Database Administration", Level = "Beginner" },
                new CourseDto { CourseId = 105, CourseName = "Angular Framework Development", Level = "Advanced" },
                new CourseDto { CourseId = 106, CourseName = "Python Django Web Development", Level = "Intermediate" },
                new CourseDto { CourseId = 107, CourseName = "Java Spring Boot Development", Level = "Advanced" },
                new CourseDto { CourseId = 108, CourseName = "DevOps & Continuous Integration/Deployment", Level = "Advanced" }
            };
        }

        private List<MentorResponse> GetSampleMentors()
        {
            return new List<MentorResponse>
            {
                new MentorResponse { Id = "M001", FullName = "Nguyễn Văn Nam", Email = "nam.nguyen@fpt.edu.vn" },
                new MentorResponse { Id = "M002", FullName = "Trần Văn Hùng", Email = "hung.tran@fpt.edu.vn" },
                new MentorResponse { Id = "M003", FullName = "Lê Thị Hương", Email = "huong.le@fpt.edu.vn" },
                new MentorResponse { Id = "M004", FullName = "Phạm Văn Đức", Email = "duc.pham@fpt.edu.vn" },
                new MentorResponse { Id = "M005", FullName = "Võ Văn Tài", Email = "tai.vo@fpt.edu.vn" },
                new MentorResponse { Id = "M006", FullName = "Bùi Văn Nghĩa", Email = "nghia.bui@fpt.edu.vn" },
                new MentorResponse { Id = "M007", FullName = "Ngô Thị Phương", Email = "phuong.ngo@fpt.edu.vn" },
                new MentorResponse { Id = "M008", FullName = "Hà Văn Quý", Email = "quy.ha@fpt.edu.vn" }
            };
        }

        private List<ClassDto> GetSampleClassData()
        {
            return new List<ClassDto>
            {
                new ClassDto
                {
                    ClassId = 1,
                    ClassName = "Lập Trình C# Cơ Bản - Lớp 1",
                    CourseId = 101,
                    CourseName = "Lập Trình C# Cơ Bản",
                    MentorId = "M001",
                    MentorName = "Nguyễn Văn Nam",
                    CreatedDate = DateTime.Now.AddDays(-30),
                    IsActive = true,
                    MaxStudents = 30,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV001", StudentName = "Trần Thị Hoa", StudentEmail = "hoa.tran@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV002", StudentName = "Lê Văn Minh", StudentEmail = "minh.le@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV003", StudentName = "Phạm Thị Lan", StudentEmail = "lan.pham@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV004", StudentName = "Hoàng Văn Đức", StudentEmail = "duc.hoang@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV005", StudentName = "Nguyễn Thị Mai", StudentEmail = "mai.nguyen@fpt.edu.vn" }
                    }
                },
                new ClassDto
                {
                    ClassId = 2,
                    ClassName = "ASP.NET Core Web API - Lớp 1",
                    CourseId = 102,
                    CourseName = "ASP.NET Core Web API Development",
                    MentorId = "M002",
                    MentorName = "Trần Văn Hùng",
                    CreatedDate = DateTime.Now.AddDays(-25),
                    IsActive = true,
                    MaxStudents = 25,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV006", StudentName = "Võ Thị Thu", StudentEmail = "thu.vo@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV007", StudentName = "Đỗ Văn Tùng", StudentEmail = "tung.do@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV008", StudentName = "Bùi Thị Nga", StudentEmail = "nga.bui@fpt.edu.vn" }
                    }
                },
                new ClassDto
                {
                    ClassId = 3,
                    ClassName = "React JS Frontend - Lớp 2",
                    CourseId = 103,
                    CourseName = "React JS Frontend Development",
                    MentorId = "M003",
                    MentorName = "Lê Thị Hương",
                    CreatedDate = DateTime.Now.AddDays(-20),
                    IsActive = true,
                    MaxStudents = 20,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV009", StudentName = "Nguyễn Văn Long", StudentEmail = "long.nguyen@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV010", StudentName = "Trần Thị Linh", StudentEmail = "linh.tran@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV011", StudentName = "Phạm Văn Quang", StudentEmail = "quang.pham@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV012", StudentName = "Lê Thị Hạnh", StudentEmail = "hanh.le@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV013", StudentName = "Hoàng Văn Thái", StudentEmail = "thai.hoang@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV014", StudentName = "Nguyễn Thị Yến", StudentEmail = "yen.nguyen@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV015", StudentName = "Trần Văn Kiên", StudentEmail = "kien.tran@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV016", StudentName = "Vũ Thị Oanh", StudentEmail = "oanh.vu@fpt.edu.vn" }
                    }
                },
                new ClassDto
                {
                    ClassId = 4,
                    ClassName = "Database SQL Server - Lớp 1",
                    CourseId = 104,
                    CourseName = "SQL Server Database Administration",
                    MentorId = "M004",
                    MentorName = "Phạm Văn Đức",
                    CreatedDate = DateTime.Now.AddDays(-15),
                    IsActive = false,
                    MaxStudents = 15,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV017", StudentName = "Đặng Văn Phong", StudentEmail = "phong.dang@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV018", StudentName = "Lý Thị Mỹ", StudentEmail = "my.ly@fpt.edu.vn" }
                    }
                },
                new ClassDto
                {
                    ClassId = 5,
                    ClassName = "Angular Frontend - Lớp 1",
                    CourseId = 105,
                    CourseName = "Angular Framework Development",
                    MentorId = "M005",
                    MentorName = "Võ Văn Tài",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    IsActive = true,
                    MaxStudents = 28,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV019", StudentName = "Huỳnh Văn An", StudentEmail = "an.huynh@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV020", StudentName = "Cao Thị Bình", StudentEmail = "binh.cao@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV021", StudentName = "Đinh Văn Cường", StudentEmail = "cuong.dinh@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV022", StudentName = "Lưu Thị Dung", StudentEmail = "dung.luu@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV023", StudentName = "Trịnh Văn Em", StudentEmail = "em.trinh@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV024", StudentName = "Phan Thị Giang", StudentEmail = "giang.phan@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV025", StudentName = "Mã Văn Hải", StudentEmail = "hai.ma@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV026", StudentName = "Tô Thị Hương", StudentEmail = "huong.to@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV027", StudentName = "Dương Văn Khánh", StudentEmail = "khanh.duong@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV028", StudentName = "Đào Thị Liên", StudentEmail = "lien.dao@fpt.edu.vn" }
                    }
                },
                new ClassDto
                {
                    ClassId = 6,
                    ClassName = "Python Django - Lớp 1",
                    CourseId = 106,
                    CourseName = "Python Django Web Development",
                    MentorId = "M006",
                    MentorName = "Bùi Văn Nghĩa",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    IsActive = true,
                    MaxStudents = 22,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV029", StudentName = "Lại Văn Mạnh", StudentEmail = "manh.lai@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV030", StudentName = "Tạ Thị Nga", StudentEmail = "nga.ta@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV031", StudentName = "Vương Văn Ơn", StudentEmail = "on.vuong@fpt.edu.vn" }
                    }
                },
                new ClassDto
                {
                    ClassId = 7,
                    ClassName = "Java Spring Boot - Lớp 2",
                    CourseId = 107,
                    CourseName = "Java Spring Boot Development",
                    MentorId = "M007",
                    MentorName = "Ngô Thị Phương",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    IsActive = false,
                    MaxStudents = 35,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>()
                },
                new ClassDto
                {
                    ClassId = 8,
                    ClassName = "DevOps & CI/CD - Lớp 1",
                    CourseId = 108,
                    CourseName = "DevOps & Continuous Integration/Deployment",
                    MentorId = "M008",
                    MentorName = "Hà Văn Quý",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    IsActive = true,
                    MaxStudents = 18,
                    CreatedBy = "Admin",
                    Students = new List<ClassStudentDto>
                    {
                        new ClassStudentDto { StudentId = "SV032", StudentName = "Ông Văn Rồng", StudentEmail = "rong.ong@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV033", StudentName = "Lê Thị Sương", StudentEmail = "suong.le@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV034", StudentName = "Trương Văn Tâm", StudentEmail = "tam.truong@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV035", StudentName = "Phùng Thị Uyên", StudentEmail = "uyen.phung@fpt.edu.vn" },
                        new ClassStudentDto { StudentId = "SV036", StudentName = "Mai Văn Việt", StudentEmail = "viet.mai@fpt.edu.vn" }
                    }
                }
            };
        }
    }
}
