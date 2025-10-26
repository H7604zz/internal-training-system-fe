using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Extensions;
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
        private readonly ICategoryService _categoryService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<KhoaHocController> _logger;

        public KhoaHocController(
            ICourseService courseService, 
            IAuthService authService, 
            ICategoryService categoryService,
            IDepartmentService departmentService,
            ILogger<KhoaHocController> logger)
        {
            _courseService = courseService;
            _authService = authService;
            _categoryService = categoryService;
            _departmentService = departmentService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchTerm, string status, int page = 1)
        {
            try
            {
                // Sử dụng page size cố định từ constants cho trang chính khóa học
                var pageSize = PaginationConstants.CoursePageSize;
                
                // Gọi API với các tham số filter và pagination - backend sẽ xử lý
                var pagedResult = await _courseService.GetCoursesAsync(
                    search: searchTerm,
                    status: status,
                    page: page,
                    pageSize: pageSize
                );

                // Lấy dữ liệu từ PagedResult
                var courses = pagedResult.Items?.ToList() ?? new List<CourseDto>();
                var totalItems = pagedResult.TotalCount;
                var totalPages = pagedResult.TotalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems; // Tổng số bản ghi từ DB (đã được filter)
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Status = status;

                return View(courses);
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

                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();

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
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
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
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating course {model.CourseId}");
                TempData["Error"] = "Đã xảy ra lỗi khi cập nhật khóa học.";
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
                return View(model);
            }
        }

        [HttpGet("tao-moi")]
        public async Task<IActionResult> TaoMoi()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(new CreateFullCourseDto());
        }

        [HttpPost]
        public async Task<IActionResult> TaoMoi(CreateFullCourseDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
                    return View(model);
                }

                var result = await _courseService.CreateFullCourseAsync(model);
                if (result != null && result.Success)
                {
                    TempData["Success"] = "Thêm khóa học mới thành công! Khóa học đã được gửi để chờ phê duyệt.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = result?.Message ?? "Không thể tạo khóa học. Vui lòng thử lại.";
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new course");
                TempData["Error"] = "Đã xảy ra lỗi khi thêm khóa học.";
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
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
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var selectedDepartments = departments.Where(d => selectedDepartmentIds.Contains(d.DepartmentId)).ToList();

                // Simulate employee count (in real application, this would come from employee service)
                var totalEmployees = selectedDepartments.Sum(d => GetEmployeeCountByDepartment(d.DepartmentId));

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
                    departments = selectedDepartments.Select(d => d.DepartmentName).ToArray()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi thông báo. Vui lòng thử lại!" });
            }
        }

        [HttpGet("danh-sach-nhan-vien/{id}")]
        public async Task<IActionResult> DanhSachNhanVien(int id, string employeeId = "", string status = "", int page = 1)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                // Sử dụng page size cố định từ constants
                var pageSize = PaginationConstants.CoursePageSize;

                // Gọi API để lấy danh sách nhân viên đủ điều kiện với filters
                var eligibleStaffResult = await _courseService.GetEligibleStaffAsync(id, page, pageSize, employeeId, status);
                var staff = eligibleStaffResult.Items?.ToList() ?? new List<EligibleStaffDto>();

                ViewBag.Course = course;
                ViewBag.CourseId = id;
                ViewBag.EmployeeId = employeeId;
                ViewBag.Status = status;
                
                // Statistics từ dữ liệu đã được filter từ API
                ViewBag.TotalEmployees = eligibleStaffResult.TotalCount; // Tổng từ API
                ViewBag.EnrolledCount = staff.Count(r => r.Status.IsEnrolled());
                ViewBag.NotEnrolledCount = staff.Count(r => r.Status.IsNotEnrolled());
                
                // Phân trang - sử dụng tương tự như Index action
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = eligibleStaffResult.TotalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = eligibleStaffResult.TotalCount; // Tổng số bản ghi từ API
                ViewBag.HasPreviousPage = eligibleStaffResult.HasPreviousPage;
                ViewBag.HasNextPage = eligibleStaffResult.HasNextPage;

                return View(staff);
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
                var filteredCourses = status switch
                {
                    var s when s.Equals(EmployeeResponseType.Accepted, StringComparison.OrdinalIgnoreCase) => allCourses.Where(c => c.ResponseType == EmployeeResponseType.Accepted).ToList(),
                    var s when s.Equals(EmployeeResponseType.Declined, StringComparison.OrdinalIgnoreCase) => allCourses.Where(c => c.ResponseType == EmployeeResponseType.Declined).ToList(),
                    var s when s.Equals(EmployeeResponseType.Pending, StringComparison.OrdinalIgnoreCase) => allCourses.Where(c => c.ResponseType == EmployeeResponseType.Pending).ToList(),
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
