using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("khoa-hoc")]
    public class KhoaHocController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<KhoaHocController> _logger;

        public KhoaHocController(ICourseService courseService, ILogger<KhoaHocController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 9, string searchTerm = "", int categoryId = 0, int departmentId = 0)
        {
            try
            {
                // var allCourses = await _courseService.GetCoursesAsync();
                var allCourses = GetSampleCourseData(); // Sử dụng data mẫu để test
                
                // Filter theo search term
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    allCourses = allCourses.Where(c => 
                        c.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true
                    ).ToList();
                }
                
                // Filter theo category
                if (categoryId > 0)
                {
                    allCourses = allCourses.Where(c => c.CategoryName.Contains(GetCategoryName(categoryId))).ToList();
                }
                
                // Filter theo department
                if (departmentId > 0)
                {
                    allCourses = allCourses.Where(c => 
                        c.Departments.Any(d => d.Id == departmentId)
                    ).ToList();
                }

                var totalItems = allCourses.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                
                var pagedCourses = allCourses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoryId = categoryId;
                ViewBag.DepartmentId = departmentId;
                
                // Data cho dropdowns
                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();

                return View(pagedCourses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses list");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách khóa học.";
                return View(new List<CourseDto>());
            }
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                // var course = await _courseService.GetCourseByIdAsync(id);
                var allCourses = GetSampleCourseData(); // Sử dụng data mẫu để test
                var course = allCourses.FirstOrDefault(c => c.CourseId == id);

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course detail with ID: {CourseId}", id);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết khóa học.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> ChinhSua(int id)
        {
            try
            {
                // var course = await _courseService.GetCourseByIdAsync(id);
                var allCourses = GetSampleCourseData(); // Sử dụng data mẫu để test
                var course = allCourses.FirstOrDefault(c => c.CourseId == id);

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course for edit with ID: {CourseId}", id);
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChinhSua(CourseDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = GetCategories();
                    ViewBag.Departments = GetDepartments();
                    return View(model);
                }

                // await _courseService.UpdateCourseAsync(model);
                TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
                return RedirectToAction("ChiTiet", new { id = model.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating course with ID: {CourseId}", model.CourseId);
                TempData["Error"] = "Đã xảy ra lỗi khi cập nhật khóa học.";
                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();
                return View(model);
            }
        }

        [HttpGet("tao-moi")]
        public IActionResult TaoMoi()
        {
            ViewBag.Categories = GetCategories();
            ViewBag.Departments = GetDepartments();
            return View(new CourseDto());
        }

        [HttpPost("tao-moi")]
        public async Task<IActionResult> TaoMoi(CourseDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = GetCategories();
                    ViewBag.Departments = GetDepartments();
                    return View(model);
                }

                // await _courseService.CreateCourseAsync(model);
                TempData["SuccessMessage"] = "Thêm khóa học mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating course");
                TempData["Error"] = "Đã xảy ra lỗi khi thêm khóa học.";
                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();
                return View(model);
            }
        }

        private List<CourseDto> GetSampleCourseData()
        {
            var departments = GetDepartments();
            
            return new List<CourseDto>
            {
                new CourseDto
                {
                    CourseId = 1,
                    CourseName = "Lập Trình C# Cơ Bản",
                    Code = "CS001",
                    Description = "Khóa học cung cấp kiến thức nền tảng về ngôn ngữ lập trình C#, bao gồm cú pháp, OOP, và các khái niệm cơ bản.",
                    Duration = 40,
                    Level = "Cơ bản",
                    CategoryName = "Lập trình",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-30),
                    CreatedBy = "Admin",
                    Departments = departments.Take(2).ToList()
                },
                new CourseDto
                {
                    CourseId = 2,
                    CourseName = "ASP.NET Core Web API Development",
                    Code = "API001",
                    Description = "Khóa học chuyên sâu về phát triển Web API sử dụng ASP.NET Core, bao gồm RESTful API, Authentication, Authorization.",
                    Duration = 60,
                    Level = "Trung cấp",
                    CategoryName = "Web Development",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-25),
                    CreatedBy = "Admin",
                    Departments = departments.Where(d => d.Name.Contains("IT") || d.Name.Contains("Tech")).ToList()
                },
                new CourseDto
                {
                    CourseId = 3,
                    CourseName = "React JS Frontend Master",
                    Code = "FE001",
                    Description = "Khóa học toàn diện về React JS, từ cơ bản đến nâng cao, bao gồm Hooks, Context API, Redux, và các best practices.",
                    Duration = 50,
                    Level = "Nâng cao",
                    CategoryName = "Frontend",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-20),
                    CreatedBy = "Admin",
                    Departments = departments.Where(d => d.Name.Contains("IT")).ToList()
                },
                new CourseDto
                {
                    CourseId = 4,
                    CourseName = "Database Design & SQL Server",
                    Code = "DB001",
                    Description = "Khóa học về thiết kế cơ sở dữ liệu và quản trị SQL Server, bao gồm normalization, indexing, stored procedures.",
                    Duration = 45,
                    Level = "Trung cấp",
                    CategoryName = "Database",
                    IsActive = false,
                    CreatedDate = DateTime.Now.AddDays(-15),
                    CreatedBy = "Admin",
                    Departments = departments.Take(3).ToList()
                },
                new CourseDto
                {
                    CourseId = 5,
                    CourseName = "Angular Framework Complete Guide",
                    Code = "NG001",
                    Description = "Khóa học đầy đủ về Angular framework, từ cơ bản đến nâng cao, bao gồm TypeScript, RxJS, và Angular Material.",
                    Duration = 55,
                    Level = "Trung cấp",
                    CategoryName = "Frontend",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-10),
                    CreatedBy = "Admin",
                    Departments = departments.Where(d => d.Name.Contains("IT") || d.Name.Contains("Dev")).ToList()
                },
                new CourseDto
                {
                    CourseId = 6,
                    CourseName = "Python for Data Science",
                    Code = "PY001",
                    Description = "Khóa học Python ứng dụng trong Data Science, bao gồm pandas, numpy, matplotlib, scikit-learn.",
                    Duration = 65,
                    Level = "Nâng cao",
                    CategoryName = "Data Science",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    CreatedBy = "Admin",
                    Departments = departments.Where(d => d.Name.Contains("Data") || d.Name.Contains("Analytics")).ToList()
                },
                new CourseDto
                {
                    CourseId = 7,
                    CourseName = "DevOps & CI/CD Pipeline",
                    Code = "DO001",
                    Description = "Khóa học về DevOps practices, CI/CD pipeline, Docker, Kubernetes, và cloud deployment.",
                    Duration = 70,
                    Level = "Nâng cao",
                    CategoryName = "DevOps",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-2),
                    CreatedBy = "Admin",
                    Departments = departments.Take(4).ToList()
                },
                new CourseDto
                {
                    CourseId = 8,
                    CourseName = "Mobile App Development with Flutter",
                    Code = "FL001",
                    Description = "Khóa học phát triển ứng dụng di động đa nền tảng sử dụng Flutter framework và Dart language.",
                    Duration = 60,
                    Level = "Trung cấp",
                    CategoryName = "Mobile Development",
                    IsActive = false,
                    CreatedDate = DateTime.Now.AddDays(-1),
                    CreatedBy = "Admin",
                    Departments = departments.Where(d => d.Name.Contains("Mobile") || d.Name.Contains("IT")).ToList()
                }
            };
        }

        private List<DepartmentDto> GetDepartments()
        {
            return new List<DepartmentDto>
            {
                new DepartmentDto { Id = 1, Name = "IT Department", Code = "IT", IsActive = true },
                new DepartmentDto { Id = 2, Name = "Software Development", Code = "DEV", IsActive = true },
                new DepartmentDto { Id = 3, Name = "Data Analytics", Code = "DATA", IsActive = true },
                new DepartmentDto { Id = 4, Name = "QA Testing", Code = "QA", IsActive = true },
                new DepartmentDto { Id = 5, Name = "Technical Support", Code = "TECH", IsActive = true },
                new DepartmentDto { Id = 6, Name = "Mobile Development", Code = "MOBILE", IsActive = true }
            };
        }

        private List<object> GetCategories()
        {
            return new List<object>
            {
                new { Id = 1, Name = "Lập trình" },
                new { Id = 2, Name = "Web Development" },
                new { Id = 3, Name = "Frontend" },
                new { Id = 4, Name = "Database" },
                new { Id = 5, Name = "Data Science" },
                new { Id = 6, Name = "DevOps" },
                new { Id = 7, Name = "Mobile Development" }
            };
        }

        private string GetCategoryName(int categoryId)
        {
            var categories = GetCategories();
            var category = categories.FirstOrDefault(c => ((dynamic)c).Id == categoryId);
            return category != null ? ((dynamic)category).Name : "";
        }
    }
}
