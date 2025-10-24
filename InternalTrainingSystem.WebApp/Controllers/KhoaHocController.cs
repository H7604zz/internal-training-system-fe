using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using InternalTrainingSystem.WebApp.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using InternalTrainingSystem.WebApp.Services.Implement;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("khoa-hoc")]
    public class KhoaHocController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IAuthService _authService;
        private readonly ILogger<KhoaHocController> _logger;

        public KhoaHocController(ICourseService courseService, IAuthService authService, ILogger<KhoaHocController> logger)
        {
            _courseService = courseService;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchTerm, string status, int page = 1)
        {
            try
            {
                // Sử dụng page size cố định từ constants cho trang chính khóa học
                var pageSize = PaginationConstants.CoursePageSize;
                
                var pagedResult = await _courseService.GetCoursesAsync(page, pageSize);

                var allCourses = pagedResult.Items.ToList();

                // Filter theo search term (nếu cần filter ở client side)
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    allCourses = allCourses.Where(c =>
                        c.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true
                    ).ToList();
                }

                // Filter theo status (nếu cần filter ở client side)
                if (!string.IsNullOrEmpty(status))
                {
                    allCourses = allCourses.Where(c => 
                        c.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true
                    ).ToList();
                }

                // TotalItems từ DB (không phải đếm theo trang hiện tại)
                var totalItems = pagedResult.TotalCount;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems; // Tổng số bản ghi trong DB
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Status = status;

                // Data cho dropdowns - giữ lại để sử dụng ở các chỗ khác
                ViewBag.Categories = GetCategories();
                ViewBag.Departments = GetDepartments();

                return View(allCourses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading course list");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách khóa học.";
                return View(new List<CourseDto>());
            }
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);

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
                var course = await _courseService.GetCourseByIdAsync(id);

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
                _logger.LogError(ex, $"Error occurred while loading course {id} for editing");
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

                var result = await _courseService.UpdateCourseAsync(model);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
                    return RedirectToAction("ChiTiet", new { id = model.CourseId });
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật khóa học. Vui lòng thử lại.";
                    ViewBag.Categories = GetCategories();
                    ViewBag.Departments = GetDepartments();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating course {model.CourseId}");
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

                var result = await _courseService.CreateCourseAsync(model);
                if (result != null)
                {
                    TempData["Success"] = "Thêm khóa học mới thành công! Khóa học đã được gửi để chờ phê duyệt.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Không thể tạo khóa học. Vui lòng thử lại.";
                    ViewBag.Categories = GetCategories();
                    ViewBag.Departments = GetDepartments();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new course");
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
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                // TODO: Replace with actual API call to get employee responses
                // For now, return empty list until employee response API is implemented
                var responses = new List<EmployeeResponseDto>();

                // Apply filters when data is available
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
                _logger.LogError(ex, $"Error occurred while getting employee list for course {id}");
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
                var course = await _courseService.GetCourseByIdAsync(id);

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
                ViewBag.ApprovalHistory = await GetApprovalHistory(id);

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while loading course {id} for approval");
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

                var result = await _courseService.ApproveCourseAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while approving course {request.CourseId}");
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

                var result = await _courseService.RejectCourseAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while rejecting course {request.CourseId}");
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi từ chối khóa học. Vui lòng thử lại.",
                    ErrorCode = ErrorCode.InternalError
                });
            }
        }



        private async Task<List<ApprovalHistoryDto>> GetApprovalHistory(int courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting approval history for course {courseId}");
                return new List<ApprovalHistoryDto>();
            }
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
                var employeeId = HttpContext.Session.GetString("EmployeeId") ?? "He173343"; // fallback for testing
                if (string.IsNullOrEmpty(employeeId))
                {
                    TempData["Error"] = "Không tìm thấy thông tin nhân viên.";
                    return RedirectToAction("Index", "TrangChu");
                }

                // Lấy danh sách khóa học từ API
                var allCourses = await _courseService.GetEmployeeCoursesAsync(employeeId);

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
                _logger.LogError(ex, $"Error occurred while getting employee courses");
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

                var result = await _courseService.RespondToCourseAsync(courseId, employeeId, responseType, note);

                if (result)
                {
                    var message = responseType.ToLower() switch
                    {
                        "accepted" => "Đã xác nhận tham gia khóa học thành công!",
                        "declined" => "Đã từ chối tham gia khóa học.",
                        _ => "Đã cập nhật phản hồi thành công!"
                    };

                    return Json(new { success = true, message = message });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật phản hồi. Vui lòng thử lại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while responding to course {courseId} for employee {HttpContext.Session.GetString("EmployeeId")}");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phản hồi" });
            }
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

                // TODO: Implement actual API call when manager decision endpoint is available
                // var result = await _courseService.SaveManagerDecisionAsync(request);

                // Simulate API call success for now
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
                _logger.LogError(ex, "Error in ManagerDecision");
                
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
