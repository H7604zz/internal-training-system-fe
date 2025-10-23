using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using InternalTrainingSystem.WebApp.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using InternalTrainingSystem.WebApp.Services.Implement;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("khoa-hoc")]
    public class KhoaHocController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IAuthService _authService;

        public KhoaHocController(ICourseService courseService, IAuthService authService)
        {
            _courseService = courseService;
            _authService = authService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "", string status = "")
        {
            try
            {
                // Sử dụng page size cố định từ constants cho trang chính khóa học
                var pageSize = PaginationConstants.CoursePageSize;
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

                // Filter theo status
                if (!string.IsNullOrEmpty(status))
                {
                    allCourses = allCourses.Where(c => 
                        c.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true
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
                ViewBag.Status = status;

                // Data cho dropdowns - giữ lại để sử dụng ở các chỗ khác
                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();

                return View(pagedCourses);
            }
            catch (Exception ex)
            {
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
                TempData["Error"] = "Đã xảy ra lỗi khi thêm khóa học.";
                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();
                return View(model);
            }
        }

        [HttpPost("gui-thong-bao")]
        public async Task<IActionResult> GuiThongBao([FromForm] string courseName, [FromForm] string description, [FromForm] List<int> selectedDepartmentIds)
        {
            try
            {
                if (string.IsNullOrEmpty(courseName))
                {
                    return Json(new { success = false, message = "Tên khóa học không được để trống!" });
                }

                if (selectedDepartmentIds == null || !selectedDepartmentIds.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một phòng ban!" });
                }

                // Simulate getting eligible employees from selected departments
                var departments = GetDepartments();
                var selectedDepartments = departments.Where(d => selectedDepartmentIds.Contains(d.Id)).ToList();

                // Simulate employee count (in real application, this would come from employee service)
                var totalEmployees = selectedDepartments.Sum(d => GetEmployeeCountByDepartment(d.Id));

                // Simulate sending notifications
                await Task.Delay(1000); // Simulate processing time

                // Here you would implement actual notification logic:
                // - Get eligible employees from selected departments
                // - Create notification records
                // - Send emails/push notifications
                // - Save notification history
                return Json(new {
                    success = true,
                    message = "Gửi thông báo thành công!",
                    sentCount = totalEmployees,
                    departments = selectedDepartments.Select(d => d.Name).ToArray()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi thông báo. Vui lòng thử lại!" });
            }
        }

        [HttpGet("danh-sach-nhan-vien/{id}")]
        public async Task<IActionResult> DanhSachNhanVien(int id, string employeeId = "", int departmentId = 0)
        {
            try
            {
                var course = GetSampleCourseData().FirstOrDefault(c => c.CourseId == id);
                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                // Get employee responses for this course
                var responses = GetSampleEmployeeResponses(id);

                // Sort by priority: NotInvited first, then others
                responses = responses.OrderBy(r => r.ResponseType == EmployeeResponseType.NotInvited ? 0 : 1)
                                   .ThenBy(r => r.EmployeeName)
                                   .ToList();

                // Apply filters
                if (!string.IsNullOrEmpty(employeeId))
                {
                    responses = responses.Where(r => r.EmployeeId.ToString().Contains(employeeId, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (departmentId > 0)
                {
                    var departments = GetDepartments();
                    var selectedDept = departments.FirstOrDefault(d => d.Id == departmentId);
                    if (selectedDept != null)
                    {
                        responses = responses.Where(r => r.DepartmentName.Contains(selectedDept.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }

                ViewBag.Course = course;
                ViewBag.CourseId = id;
                ViewBag.EmployeeId = employeeId;
                ViewBag.DepartmentId = departmentId;
                ViewBag.Departments = GetDepartments();
                ViewBag.TotalEmployees = responses.Count;
                ViewBag.AcceptedCount = responses.Count(r => r.ResponseType == EmployeeResponseType.Accepted);
                ViewBag.DeclinedCount = responses.Count(r => r.ResponseType == EmployeeResponseType.Declined);
                ViewBag.PendingCount = responses.Count(r => r.ResponseType == EmployeeResponseType.Pending);
                ViewBag.NotInvitedCount = responses.Count(r => r.ResponseType == EmployeeResponseType.NotInvited);

                return View(responses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách nhân viên.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("reinvite-employee")]
        public async Task<IActionResult> ReinviteEmployee([FromBody] ReinviteEmployeeRequest request)
        {
            try
            {
                if (request.CourseId <= 0 || request.EmployeeId <= 0)
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ." });
                }

                // Simulate processing time
                await Task.Delay(1000);

                // Here you would implement actual reinvite logic:
                // - Update employee status to "Pending" 
                // - Send new notification email
                // - Log the reinvite action
                return Json(new { success = true, message = "Đã gửi lại lời mời thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi lời mời. Vui lòng thử lại!" });
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

                if (course.Status != CourseStatus.Pending)
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
                        ErrorCode = ErrorCode.InvalidRequest
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
                        ErrorCode = ErrorCode.CourseNotFound
                    });
                }

                if (request.Action.ToLower() == ApprovalAction.Approve)
                {
                    // Simulate approval

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
                    ErrorCode = ErrorCode.InvalidAction
                });
            }
            catch (Exception ex)
            {
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi phê duyệt khóa học. Vui lòng thử lại.",
                    ErrorCode = ErrorCode.InternalError
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
                        ErrorCode = ErrorCode.InvalidRequest
                    });
                }

                if (request.Reason.Length < 20)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Lý do từ chối phải có ít nhất 20 ký tự.",
                        ErrorCode = ErrorCode.ReasonTooShort
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
                        ErrorCode = ErrorCode.CourseNotFound
                    });
                }

                if (request.Action.ToLower() == "reject")
                {
                    // Simulate rejection
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
                    ErrorCode = ErrorCode.InvalidAction
                });
            }
            catch (Exception ex)
            {
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi từ chối khóa học. Vui lòng thử lại.",
                    ErrorCode = ErrorCode.InternalError
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
                    IsOnline = false,
                    IsMandatory = true,
                    CreatedDate = DateTime.Now.AddDays(-30),
                    CreatedBy = "Nguyễn Văn Nam",
                    Status = CourseStatus.Approved,
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
                    IsOnline = true,
                    IsMandatory = false,
                    CreatedDate = DateTime.Now.AddDays(-25),
                    CreatedBy = "Lê Minh Đức",
                    Status = CourseStatus.Approved,
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
                    IsOnline = true,
                    IsMandatory = false,
                    CreatedDate = DateTime.Now.AddDays(-20),
                    CreatedBy = "Phạm Thị Mai",
                    Status = CourseStatus.Approved,
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
                    IsOnline = false,
                    IsMandatory = true,
                    CreatedDate = DateTime.Now.AddDays(-15),
                    CreatedBy = "Hoàng Văn Tùng",
                    Status = CourseStatus.Rejected,
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
                    IsOnline = true,
                    IsMandatory = false,
                    CreatedDate = DateTime.Now.AddDays(-10),
                    CreatedBy = "Vũ Thị Lan",
                    Status = CourseStatus.Approved,
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
                    IsOnline = false,
                    IsMandatory = true,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    CreatedBy = "Đặng Minh Quân",
                    Status = CourseStatus.Pending,
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
                    IsOnline = true,
                    IsMandatory = false,
                    CreatedDate = DateTime.Now.AddDays(-2),
                    CreatedBy = "Bùi Văn Hải",
                    Status = CourseStatus.Pending,
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
                    IsOnline = false,
                    IsMandatory = false,
                    CreatedDate = DateTime.Now.AddDays(-1),
                    CreatedBy = "Trịnh Thị Hoa",
                    Status = CourseStatus.Pending,
                    Departments = departments.Where(d => d.Name.Contains("Mobile") || d.Name.Contains("IT")).ToList()
                }
            };
        }

        private List<CourseDto> GetSampleCourseDataForApproval()
        {
            var allCourses = GetSampleCourseData();
            // Return courses that are pending approval for the approval view
            return allCourses.Where(c => c.Status == CourseStatus.Pending).ToList();
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
                    Action = ApprovalAction.Created,
                    Description = "Khóa học được tạo",
                    ActionBy = course.CreatedBy ?? "Nhân viên đào tạo",
                    ActionDate = course.CreatedDate
                },
                new ApprovalHistoryDto
                {
                    Id = 2,
                    CourseId = courseId,
                    Action = ApprovalAction.Submitted,
                    Description = "Gửi yêu cầu phê duyệt",
                    Note = "Khóa học đã sẵn sàng để phê duyệt",
                    ActionBy = course.CreatedBy ?? "Nhân viên đào tạo",
                    ActionDate = course.CreatedDate.AddMinutes(30)
                }
            };

            // Add approval/rejection history if exists
            if (course.Status == CourseStatus.Approved && course.ApprovalDate.HasValue)
            {
                history.Add(new ApprovalHistoryDto
                {
                    Id = 3,
                    CourseId = courseId,
                    Action = ApprovalAction.Approved,
                    Description = "Khóa học đã được phê duyệt",
                    Note = "Khóa học đã được phê duyệt và có thể tạo lớp học",
                    ActionBy = course.ApprovedBy ?? "Giám đốc",
                    ActionDate = course.ApprovalDate.Value
                });
            }
            else if (course.Status == CourseStatus.Rejected && course.ApprovalDate.HasValue)
            {
                history.Add(new ApprovalHistoryDto
                {
                    Id = 3,
                    CourseId = courseId,
                    Action = ApprovalAction.Rejected,
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

        private int GetEmployeeCountByDepartment(int departmentId)
        {
            // Simulate employee count per department
            var employeeCounts = new Dictionary<int, int>
            {
                { 1, 15 }, // IT Department
                { 2, 20 }, // Software Development
                { 3, 12 }, // Data Analytics
                { 4, 8 },  // QA Testing
                { 5, 10 }, // Technical Support
                { 6, 6 }   // Mobile Development
            };

            return employeeCounts.ContainsKey(departmentId) ? employeeCounts[departmentId] : 0;
        }

        private List<EmployeeResponseDto> GetSampleEmployeeResponses(int courseId)
        {
            return new List<EmployeeResponseDto>
            {
                new EmployeeResponseDto
                {
                    Id = 1,
                    CourseId = courseId,
                    EmployeeId = 101,
                    EmployeeName = "Nguyễn Văn An",
                    DepartmentName = "IT Department",
                    Position = "Senior Developer",
                    ResponseType = EmployeeResponseType.Accepted,
                    ResponseDate = DateTime.Now.AddHours(-2),
                    Note = "Tôi rất quan tâm đến khóa học này và mong muốn tham gia.",
                    ContactEmail = "an.nguyen@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 2,
                    CourseId = courseId,
                    EmployeeId = 102,
                    EmployeeName = "Trần Thị Bình",
                    DepartmentName = "Software Development",
                    Position = "Frontend Developer",
                    ResponseType = EmployeeResponseType.Accepted,
                    ResponseDate = DateTime.Now.AddHours(-1),
                    Note = "Khóa học phù hợp với công việc hiện tại của tôi.",
                    ContactEmail = "binh.tran@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 3,
                    CourseId = courseId,
                    EmployeeId = 103,
                    EmployeeName = "Lê Minh Cường",
                    DepartmentName = "Data Analytics",
                    Position = "Data Analyst",
                    ResponseType = EmployeeResponseType.Declined,
                    ResponseDate = DateTime.Now.AddMinutes(-30),
                    Note = "Hiện tại tôi đang có dự án quan trọng, không thể tham gia lúc này.",
                    ContactEmail = "cuong.le@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 4,
                    CourseId = courseId,
                    EmployeeId = 104,
                    EmployeeName = "Phạm Thị Dung",
                    DepartmentName = "QA Testing",
                    Position = "QA Engineer",
                    ResponseType = EmployeeResponseType.Pending,
                    ResponseDate = null,
                    Note = null,
                    ContactEmail = "dung.pham@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 5,
                    CourseId = courseId,
                    EmployeeId = 105,
                    EmployeeName = "Vũ Văn Em",
                    DepartmentName = "Technical Support",
                    Position = "Support Engineer",
                    ResponseType = EmployeeResponseType.Accepted,
                    ResponseDate = DateTime.Now.AddMinutes(-15),
                    Note = "Tôi muốn nâng cao kỹ năng kỹ thuật của mình.",
                    ContactEmail = "em.vu@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 6,
                    CourseId = courseId,
                    EmployeeId = 106,
                    EmployeeName = "Hoàng Thị Phương",
                    DepartmentName = "Mobile Development",
                    Position = "Mobile Developer",
                    ResponseType = EmployeeResponseType.Pending,
                    ResponseDate = null,
                    Note = null,
                    ContactEmail = "phuong.hoang@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 7,
                    CourseId = courseId,
                    EmployeeId = 107,
                    EmployeeName = "Đặng Văn Giang",
                    DepartmentName = "IT Department",
                    Position = "System Administrator",
                    ResponseType = EmployeeResponseType.Accepted,
                    ResponseDate = DateTime.Now.AddMinutes(-45),
                    Note = "Khóa học này sẽ giúp tôi hiểu rõ hơn về hệ thống.",
                    ContactEmail = "giang.dang@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 8,
                    CourseId = courseId,
                    EmployeeId = 108,
                    EmployeeName = "Bùi Thị Hạnh",
                    DepartmentName = "Software Development",
                    Position = "Backend Developer",
                    ResponseType = EmployeeResponseType.Declined,
                    ResponseDate = DateTime.Now.AddHours(-3),
                    Note = "Tôi đã có kiến thức về chủ đề này rồi.",
                    ContactEmail = "hanh.bui@company.com"
                },
                // Nh�n vi�n m?i - chua du?c m?i
                new EmployeeResponseDto
                {
                    Id = 9,
                    CourseId = courseId,
                    EmployeeId = 109,
                    EmployeeName = "Trần Minh Khôi",
                    DepartmentName = "IT Department",
                    Position = "Junior Developer",
                    ResponseType = EmployeeResponseType.NotInvited,
                    ResponseDate = null,
                    Note = null,
                    ContactEmail = "khoi.tran@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 10,
                    CourseId = courseId,
                    EmployeeId = 110,
                    EmployeeName = "Nguyễn Thị Lan",
                    DepartmentName = "Software Development",
                    Position = "Junior Frontend Developer",
                    ResponseType = EmployeeResponseType.NotInvited,
                    ResponseDate = null,
                    Note = null,
                    ContactEmail = "lan.nguyen@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 11,
                    CourseId = courseId,
                    EmployeeId = 111,
                    EmployeeName = "Lê Văn Minh",
                    DepartmentName = "Data Analytics",
                    Position = "Data Entry Specialist",
                    ResponseType = EmployeeResponseType.NotInvited,
                    ResponseDate = null,
                    Note = null,
                    ContactEmail = "minh.le@company.com"
                },
                new EmployeeResponseDto
                {
                    Id = 12,
                    CourseId = courseId,
                    EmployeeId = 112,
                    EmployeeName = "Phạm Văn Nam",
                    DepartmentName = "QA Testing",
                    Position = "Manual Tester",
                    ResponseType = EmployeeResponseType.NotInvited,
                    ResponseDate = null,
                    Note = null,
                    ContactEmail = "nam.pham@company.com"
                }
            };
        }
    

        /// <summary>
        /// Danh sách khóa học của nhân viên
        /// </summary>
        [HttpGet("danh-sach-khoa-hoc-cua-toi")]
        public async Task<IActionResult> DanhSachKhoaHocCuaToi(string status = "all", int page = 1)
        {
            try
            {
                // Sử dụng page size cố định từ constants cho khóa học của nhân viên
                var pageSize = PaginationConstants.EmployeeCoursePageSize;
                
                // Kiểm tra đăng nhập
                if (_authService.IsTokenExpired())
                {
                    return RedirectToAction("DangNhap", "Auth");
                }

                // Lấy thông tin nhân viên từ session
                //var employeeId = HttpContext.Session.GetString("EmployeeId");
                var employeeId = "He173343"; //test data
                if (string.IsNullOrEmpty(employeeId))
                {
                    TempData["Error"] = "Không tìm thấy thông tin nhân viên.";
                    return RedirectToAction("Index", "TrangChu");
                }

                // Lấy danh sách khóa học (dùng dữ liệu mẫu cho demo)
                var allCourses = GetSampleEmployeeCourses(employeeId);

                // Lọc theo trạng thái
                var filteredCourses = status.ToLower() switch
                {
                    "accepted" => allCourses.Where(c => c.ResponseType == "Accepted").ToList(),
                    "declined" => allCourses.Where(c => c.ResponseType == "Declined").ToList(),
                    "pending" => allCourses.Where(c => c.ResponseType == "Pending").ToList(),
                    _ => allCourses
                };

                // Phân trang
                var totalItems = filteredCourses.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var pagedCourses = filteredCourses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Thống kê
                ViewBag.TotalCourses = allCourses.Count;
                ViewBag.AcceptedCount = allCourses.Count(c => c.ResponseType == "Accepted");
                ViewBag.DeclinedCount = allCourses.Count(c => c.ResponseType == "Declined");
                ViewBag.PendingCount = allCourses.Count(c => c.ResponseType == "Pending");

                // Phân trang
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentStatus = status;

                return View(pagedCourses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách khóa học.";
                return RedirectToAction("Index", "TrangChu");
            }
        }

        /// <summary>
        /// Phản hồi tham gia khóa học
        /// </summary>
        [HttpPost("phan-hoi-khoa-hoc")]
        public async Task<IActionResult> PhanHoiKhoaHoc(int courseId, string responseType, string? note = null)
        {
            try
            {
                if (_authService.IsTokenExpired())
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var employeeId = HttpContext.Session.GetString("EmployeeId");
                if (string.IsNullOrEmpty(employeeId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                // TODO: Gọi API để cập nhật phản hồi
                // var result = await _courseService.RespondToCourseAsync(courseId, employeeId, responseType, note);

                // Simulate success response
                var message = responseType.ToLower() switch
                {
                    "accepted" => "Đã xác nhận tham gia khóa học thành công!",
                    "declined" => "Đã từ chối tham gia khóa học.",
                    _ => "Đã cập nhật phản hồi thành công!"
                };

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phản hồi" });
            }
        }

        /// <summary>
        /// Sample data for testing employee courses
        /// </summary>
        private List<EmployeeCourseDto> GetSampleEmployeeCourses(string employeeId)
        {
            return new List<EmployeeCourseDto>
            {
                new EmployeeCourseDto
                {
                    CourseId = 1,
                    CourseCode = "COURSE001",
                    CourseName = "Khóa học lập trình C# nâng cao",
                    Description = "Khóa học lập trình C# từ cơ bản đến nâng cao, bao gồm ASP.NET Core, Entity Framework",
                    Duration = "40 giờ",
                    Level = CourseConstants.Levels.Advanced,
                    StartDate = DateTime.Now.AddDays(15),
                    EndDate = DateTime.Now.AddDays(45),
                    TrainerName = "Nguyễn Văn Trainer",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Pending",
                    ResponseDate = null,
                    Note = null,
                    InvitedDate = DateTime.Now.AddDays(-3),
                    MaxParticipants = 20,
                    CurrentParticipants = 15,
                    Status = "Open"
                },
                new EmployeeCourseDto
                {
                    CourseId = 2,
                    CourseCode = "COURSE002",
                    CourseName = "Khóa học React.js Frontend Development",
                    Description = "Học React.js để xây dựng ứng dụng web hiện đại, bao gồm Hooks, Context API, Redux",
                    Duration = "32 giờ",
                    Level = CourseConstants.Levels.Intermediate,
                    StartDate = DateTime.Now.AddDays(-20),
                    EndDate = DateTime.Now.AddDays(-5),
                    TrainerName = "Trần Thị Frontend",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Accepted",
                    ResponseDate = DateTime.Now.AddDays(-18),
                    Note = "Rất hứng thú với khóa học này",
                    InvitedDate = DateTime.Now.AddDays(-25),
                    MaxParticipants = 15,
                    CurrentParticipants = 12,
                    Status = "Completed"
                },
                new EmployeeCourseDto
                {
                    CourseId = 3,
                    CourseCode = "COURSE003",
                    CourseName = "Khóa học Database Design & SQL Server",
                    Description = "Thiết kế cơ sở dữ liệu và tối ưu hóa hiệu suất SQL Server",
                    Duration = "24 giờ",
                    Level = CourseConstants.Levels.Advanced,
                    StartDate = DateTime.Now.AddDays(-45),
                    EndDate = DateTime.Now.AddDays(-30),
                    TrainerName = "Lê Văn Database",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Declined",
                    ResponseDate = DateTime.Now.AddDays(-40),
                    Note = "Hiện tại bận project khẩn cấp, không thể tham gia",
                    InvitedDate = DateTime.Now.AddDays(-50),
                    MaxParticipants = 25,
                    CurrentParticipants = 18,
                    Status = "Completed"
                },
                new EmployeeCourseDto
                {
                    CourseId = 4,
                    CourseCode = "COURSE004",
                    CourseName = "Khóa học DevOps và CI/CD Pipeline",
                    Description = "Tìm hiểu về DevOps, Docker, Kubernetes và xây dựng CI/CD pipeline",
                    Duration = "48 giờ",
                    Level = CourseConstants.Levels.Advanced,
                    StartDate = DateTime.Now.AddDays(30),
                    EndDate = DateTime.Now.AddDays(75),
                    TrainerName = "Phạm Văn DevOps",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Pending",
                    ResponseDate = null,
                    Note = null,
                    InvitedDate = DateTime.Now.AddDays(-1),
                    MaxParticipants = 12,
                    CurrentParticipants = 8,
                    Status = "Open"
                },
                new EmployeeCourseDto
                {
                    CourseId = 5,
                    CourseCode = "COURSE005",
                    CourseName = "Khóa học Agile & Scrum Methodology",
                    Description = "Phương pháp quản lý dự án Agile và Scrum framework",
                    Duration = "16 giờ",
                    Level = CourseConstants.Levels.Beginner,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(5),
                    TrainerName = "Hoàng Thị Agile",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Accepted",
                    ResponseDate = DateTime.Now.AddDays(-8),
                    Note = "Cần thiết cho công việc hiện tại",
                    InvitedDate = DateTime.Now.AddDays(-15),
                    MaxParticipants = 30,
                    CurrentParticipants = 25,
                    Status = "InProgress"
                }
            };
        }

        /// <summary>
        /// API endpoint cho quyết định của quản lý trực tiếp
        /// </summary>
        [HttpPost("manager-decision")]
        public async Task<IActionResult> ManagerDecision([FromBody] ManagerDecisionRequest request)
        {
            try
            {
                // Validate request
                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                if (request.EmployeeId <= 0 || request.CourseId <= 0)
                {
                    return Json(new { success = false, message = "ID nhân viên hoặc khóa học không hợp lệ" });
                }

                if (string.IsNullOrEmpty(request.DecisionType) || 
                    (request.DecisionType != "Accept" && request.DecisionType != "Reject"))
                {
                    return Json(new { success = false, message = "Loại quyết định không hợp lệ" });
                }

                // Kiểm tra authentication
                if (_authService.IsTokenExpired())
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                // TODO: Gọi API backend để lưu quyết định của quản lý
                // var result = await _courseService.SaveManagerDecisionAsync(request);

                // Simulate API call success
                await Task.Delay(500); // Simulate processing time

                var decisionText = request.DecisionType == "Accept" ? "chấp nhận" : "từ chối cuối cùng";
                var message = $"Đã {decisionText} quyết định cho nhân viên thành công!";

                return Json(new { 
                    success = true, 
                    message = message,
                    data = new {
                        employeeId = request.EmployeeId,
                        courseId = request.CourseId,
                        decision = request.DecisionType,
                        note = request.ManagerNote,
                        timestamp = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in ManagerDecision: {ex.Message}");
                
                return Json(new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi xử lý quyết định. Vui lòng thử lại sau!" 
                });
            }
        }
    }

    // Request models for API endpoints
    public class ReinviteEmployeeRequest
    {
        public int CourseId { get; set; }
        public int EmployeeId { get; set; }
    }

    public class ManagerDecisionRequest
    {
        public int EmployeeId { get; set; }
        public int CourseId { get; set; }
        public string DecisionType { get; set; } = ""; // "Accept" hoặc "Reject"
        public string ManagerNote { get; set; } = "";
    }
}
