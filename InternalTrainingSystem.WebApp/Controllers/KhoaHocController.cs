using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
                TempData["Success"] = "Thêm khóa học mới thành công! Khóa học đã được gửi để chờ phê duyệt.";
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

        [HttpGet("phe-duyet/{id}")]
        public async Task<IActionResult> PheDuyet(int id)
        {
            try
            {
                // var course = await _courseService.GetCourseForApprovalAsync(id);
                var allCourses = GetSampleCourseDataForApproval(); // Sử dụng data mẫu để test
                var course = allCourses.FirstOrDefault(c => c.CourseId == id);

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học cần phê duyệt.";
                    return RedirectToAction("Index");
                }

                if (course.Status != "Pending")
                {
                    TempData["Warning"] = "Khóa học này đã được xử lý phê duyệt.";
                    return RedirectToAction("ChiTiet", new { id });
                }

                // Thêm thông tin người tạo và lịch sử phê duyệt
                ViewBag.CreatedBy = course.CreatedBy ?? "Nhân viên đào tạo";
                ViewBag.ApprovalHistory = GetApprovalHistory(id);

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course for approval with ID: {CourseId}", id);
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học để phê duyệt.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] CourseApprovalRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Action) || request.CourseId <= 0)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Thông tin yêu cầu không hợp lệ.",
                        ErrorCode = "INVALID_REQUEST"
                    });
                }

                // Simulate processing time
                await Task.Delay(1000);

                // Here you would call the actual service
                // var result = await _courseService.ApproveCourseAsync(request);

                // For demo purposes, simulate success
                var course = GetSampleCourseDataForApproval().FirstOrDefault(c => c.CourseId == request.CourseId);
                if (course == null)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy khóa học.",
                        ErrorCode = "COURSE_NOT_FOUND"
                    });
                }

                if (request.Action.ToLower() == "approve")
                {
                    // Simulate approval
                    _logger.LogInformation("Course {CourseId} approved by {ApprovedBy}", request.CourseId, request.ApprovedBy);
                    
                    return Json(new CourseApprovalResponse
                    {
                        Success = true,
                        Message = "Khóa học đã được phê duyệt thành công!"
                    });
                }

                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Hành động không được hỗ trợ.",
                    ErrorCode = "INVALID_ACTION"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while approving course with ID: {CourseId}", request.CourseId);
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi phê duyệt khóa học. Vui lòng thử lại.",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] CourseApprovalRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Action) || request.CourseId <= 0 || string.IsNullOrEmpty(request.Reason))
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Vui lòng nhập đầy đủ thông tin và lý do từ chối.",
                        ErrorCode = "INVALID_REQUEST"
                    });
                }

                if (request.Reason.Length < 20)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Lý do từ chối phải có ít nhất 20 ký tự.",
                        ErrorCode = "REASON_TOO_SHORT"
                    });
                }

                // Simulate processing time
                await Task.Delay(1000);

                // Here you would call the actual service
                // var result = await _courseService.RejectCourseAsync(request);

                // For demo purposes, simulate success
                var course = GetSampleCourseDataForApproval().FirstOrDefault(c => c.CourseId == request.CourseId);
                if (course == null)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy khóa học.",
                        ErrorCode = "COURSE_NOT_FOUND"
                    });
                }

                if (request.Action.ToLower() == "reject")
                {
                    // Simulate rejection
                    _logger.LogInformation("Course {CourseId} rejected by {RejectedBy} with reason: {Reason}", 
                        request.CourseId, request.ApprovedBy, request.Reason);
                    
                    return Json(new CourseApprovalResponse
                    {
                        Success = true,
                        Message = "Khóa học đã bị từ chối. Thông báo đã được gửi tới người tạo."
                    });
                }

                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Hành động không được hỗ trợ.",
                    ErrorCode = "INVALID_ACTION"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rejecting course with ID: {CourseId}", request.CourseId);
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi từ chối khóa học. Vui lòng thử lại.",
                    ErrorCode = "INTERNAL_ERROR"
                });
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
                    CreatedBy = "Nguyễn Văn Nam",
                    Status = "Approved",
                    ApprovedBy = "Trần Thị Hương - Giám đốc",
                    ApprovalDate = DateTime.Now.AddDays(-25),
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
                    CreatedBy = "Lê Minh Đức",
                    Status = "Approved",
                    ApprovedBy = "Trần Thị Hương - Giám đốc",
                    ApprovalDate = DateTime.Now.AddDays(-20),
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
                    CreatedBy = "Phạm Thị Mai",
                    Status = "Approved",
                    ApprovedBy = "Trần Thị Hương - Giám đốc",
                    ApprovalDate = DateTime.Now.AddDays(-15),
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
                    CreatedBy = "Hoàng Văn Tùng",
                    Status = "Rejected",
                    ApprovedBy = "Trần Thị Hương - Giám đốc",
                    ApprovalDate = DateTime.Now.AddDays(-10),
                    RejectionReason = "Nội dung khóa học chưa chi tiết, thiếu thông tin về thực hành. Vui lòng bổ sung thêm các bài lab và project thực tế.",
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
                    CreatedBy = "Vũ Thị Lan",
                    Status = "Approved",
                    ApprovedBy = "Trần Thị Hương - Giám đốc",
                    ApprovalDate = DateTime.Now.AddDays(-5),
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
                    CreatedBy = "Đặng Minh Quân",
                    Status = "Pending",
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
                    CreatedBy = "Bùi Văn Hải",
                    Status = "Pending",
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
                    CreatedBy = "Trịnh Thị Hoa",
                    Status = "Pending",
                    Departments = departments.Where(d => d.Name.Contains("Mobile") || d.Name.Contains("IT")).ToList()
                }
            };
        }

        private List<CourseDto> GetSampleCourseDataForApproval()
        {
            var allCourses = GetSampleCourseData();
            // Return courses that are pending approval for the approval view
            return allCourses.Where(c => c.Status == "Pending").ToList();
        }

        private List<ApprovalHistoryDto> GetApprovalHistory(int courseId)
        {
            var course = GetSampleCourseData().FirstOrDefault(c => c.CourseId == courseId);
            if (course == null) return new List<ApprovalHistoryDto>();

            var history = new List<ApprovalHistoryDto>
            {
                new ApprovalHistoryDto
                {
                    Id = 1,
                    CourseId = courseId,
                    Action = "Created",
                    Description = "Khóa học được tạo",
                    ActionBy = course.CreatedBy ?? "Nhân viên đào tạo",
                    ActionDate = course.CreatedDate
                },
                new ApprovalHistoryDto
                {
                    Id = 2,
                    CourseId = courseId,
                    Action = "Submitted",
                    Description = "Gửi yêu cầu phê duyệt",
                    Note = "Khóa học đã sẵn sàng để phê duyệt",
                    ActionBy = course.CreatedBy ?? "Nhân viên đào tạo",
                    ActionDate = course.CreatedDate.AddMinutes(30)
                }
            };

            // Add approval/rejection history if exists
            if (course.Status == "Approved" && course.ApprovalDate.HasValue)
            {
                history.Add(new ApprovalHistoryDto
                {
                    Id = 3,
                    CourseId = courseId,
                    Action = "Approved",
                    Description = "Khóa học đã được phê duyệt",
                    Note = "Khóa học đã được phê duyệt và có thể tạo lớp học",
                    ActionBy = course.ApprovedBy ?? "Giám đốc",
                    ActionDate = course.ApprovalDate.Value
                });
            }
            else if (course.Status == "Rejected" && course.ApprovalDate.HasValue)
            {
                history.Add(new ApprovalHistoryDto
                {
                    Id = 3,
                    CourseId = courseId,
                    Action = "Rejected",
                    Description = "Khóa học bị từ chối",
                    Note = course.RejectionReason,
                    ActionBy = course.ApprovedBy ?? "Giám đốc",
                    ActionDate = course.ApprovalDate.Value
                });
            }

            return history.OrderBy(h => h.ActionDate).ToList();
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
