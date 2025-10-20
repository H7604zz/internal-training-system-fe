using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Trang đăng nhập - URL sẽ là /Auth/dang-nhap
        /// </summary>
        public IActionResult DangNhap(string? returnUrl = null)
        {
            // Kiểm tra nếu user đã đăng nhập
            if (!_authService.IsTokenExpired())
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "TrangChu");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginRequestDto model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                var result = await _authService.LoginAsync(model);

                if (result.Success)
                {
                    // Lưu thông tin user vào session nếu cần
                    if (result.User != null)
                    {
                        HttpContext.Session.SetString("UserFullName", result.User.FullName);
                        HttpContext.Session.SetString("UserEmail", result.User.Email);
                        if (!string.IsNullOrEmpty(result.User.EmployeeId))
                        {
                            HttpContext.Session.SetString("EmployeeId", result.User.EmployeeId);
                        }
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "TrangChu");
                }
                else
                {
                    TempData["Error"] = result.Message;
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.";
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        /// <summary>
        /// Trang quên mật khẩu - URL sẽ là /Auth/quen-mat-khau
        /// </summary>
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        /// <summary>
        /// Xử lý quên mật khẩu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> QuenMatKhau(ForgotPasswordRequestDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _authService.ForgotPasswordAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = "Nếu email này tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn.";
                    return View();
                }
                else
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại.";
                return View(model);
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại (API endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (_authService.IsTokenExpired())
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var result = await _authService.GetProfileAsync();
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin người dùng" });
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        public async Task<IActionResult> DangXuat()
        {
            try
            {
                await _authService.LogoutAsync();
                // Xóa thông tin user khỏi session
                HttpContext.Session.Clear();
                
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                // Vẫn redirect về trang đăng nhập dù có lỗi
                HttpContext.Session.Clear();
                return RedirectToAction("DangNhap");
            }
        }

        /// <summary>
        /// Trang đổi mật khẩu
        /// </summary>
        public IActionResult DoiMatKhau()
        {
            // Kiểm tra đăng nhập
            if (_authService.IsTokenExpired())
            {
                return RedirectToAction("DangNhap");
            }

            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(ChangePasswordRequestDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _authService.ChangePasswordAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("DoiMatKhau");
                }
                else
                {
                    if (result.Errors != null && result.Errors.Any())
                    {
                        foreach (var error in result.Errors)
                        {
                            TempData["Error"] = error;
                        }
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi trong quá trình đổi mật khẩu. Vui lòng thử lại.";
                return View(model);
            }
        }

        /// <summary>
        /// Trang thông tin cá nhân
        /// </summary>
        public async Task<IActionResult> ThongTinCaNhan()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (_authService.IsTokenExpired())
                {
                    return RedirectToAction("DangNhap");
                }

                // Lấy thông tin user từ session hoặc API
                var userProfile = GetSampleUserProfile(); // Dùng data mẫu để test
                
                return View(userProfile);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin cá nhân.";
                return RedirectToAction("Index", "TrangChu");
            }
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CapNhatThongTin(UpdateProfileDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var userProfile = GetSampleUserProfile();
                    ViewBag.ShowUpdateForm = true;
                    return View("ThongTinCaNhan", userProfile);
                }

                // var result = await _authService.UpdateProfileAsync(model);
                // Simulate success for testing
                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("ThongTinCaNhan");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi cập nhật thông tin.";
                return RedirectToAction("ThongTinCaNhan");
            }
        }

        /// <summary>
        /// Danh sách khóa học của nhân viên
        /// </summary>
        public async Task<IActionResult> DanhSachKhoaHocCuaToi(string status = "all", int page = 1, int pageSize = 10)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (_authService.IsTokenExpired())
                {
                    return RedirectToAction("DangNhap");
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
        [HttpPost]
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
                    StartDate = DateTime.Now.AddDays(15),
                    EndDate = DateTime.Now.AddDays(45),
                    TrainerName = "Nguyễn Văn Trainer",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Pending",
                    ResponseDate = null,
                    Note = null,
                    InvitedDate = DateTime.Now.AddDays(-3),
                    Priority = "High",
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
                    StartDate = DateTime.Now.AddDays(-20),
                    EndDate = DateTime.Now.AddDays(-5),
                    TrainerName = "Trần Thị Frontend",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Accepted",
                    ResponseDate = DateTime.Now.AddDays(-18),
                    Note = "Rất hứng thú với khóa học này",
                    InvitedDate = DateTime.Now.AddDays(-25),
                    Priority = "Medium",
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
                    StartDate = DateTime.Now.AddDays(-45),
                    EndDate = DateTime.Now.AddDays(-30),
                    TrainerName = "Lê Văn Database",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Declined",
                    ResponseDate = DateTime.Now.AddDays(-40),
                    Note = "Hiện tại bận project khẩn cấp, không thể tham gia",
                    InvitedDate = DateTime.Now.AddDays(-50),
                    Priority = "Low",
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
                    StartDate = DateTime.Now.AddDays(30),
                    EndDate = DateTime.Now.AddDays(75),
                    TrainerName = "Phạm Văn DevOps",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Pending",
                    ResponseDate = null,
                    Note = null,
                    InvitedDate = DateTime.Now.AddDays(-1),
                    Priority = "High",
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
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(5),
                    TrainerName = "Hoàng Thị Agile",
                    DepartmentName = "Phòng Đào Tạo",
                    ResponseType = "Accepted",
                    ResponseDate = DateTime.Now.AddDays(-8),
                    Note = "Cần thiết cho công việc hiện tại",
                    InvitedDate = DateTime.Now.AddDays(-15),
                    Priority = "Medium",
                    MaxParticipants = 30,
                    CurrentParticipants = 25,
                    Status = "InProgress"
                }
            };
        }

        /// <summary>
        /// Sample data for testing user profile
        /// </summary>
        private UserProfileDto GetSampleUserProfile()
        {
            return new UserProfileDto
            {
                Id = "USR001",
                EmployeeId = "EMP2024001", 
                FullName = "Nguyễn Văn An",
                Email = "nguyenvanan@company.com",
                PhoneNumber = "0123456789",
                Department = "IT Department",
                Position = "Senior Developer",
                CurrentRole = "System Administrator",
                YearsOfExperience = 5,
                JoinDate = DateTime.Now.AddYears(-3),
                LastLoginDate = DateTime.Now.AddHours(-2),
                IsActive = true,
                Roles = new List<string> { "Admin", "Developer", "Trainer" },
                RoleHistory = new List<UserRoleHistoryDto>
                {
                    new UserRoleHistoryDto
                    {
                        Id = 1,
                        RoleName = "System Administrator",
                        RoleDescription = "Quản trị hệ thống toàn quyền",
                        StartDate = DateTime.Now.AddMonths(-6),
                        EndDate = null,
                        AssignedBy = "HR Manager",
                        IsCurrent = true,
                        Status = "Đang hoạt động"
                    },
                    new UserRoleHistoryDto
                    {
                        Id = 2,
                        RoleName = "Senior Developer",
                        RoleDescription = "Phát triển và duy trì hệ thống",
                        StartDate = DateTime.Now.AddYears(-2),
                        EndDate = DateTime.Now.AddMonths(-6),
                        AssignedBy = "Technical Lead",
                        IsCurrent = false,
                        Status = "Đã kết thúc"
                    },
                    new UserRoleHistoryDto
                    {
                        Id = 3,
                        RoleName = "Developer",
                        RoleDescription = "Phát triển tính năng mới",
                        StartDate = DateTime.Now.AddYears(-3),
                        EndDate = DateTime.Now.AddYears(-2),
                        AssignedBy = "Project Manager",
                        IsCurrent = false,
                        Status = "Đã kết thúc"
                    },
                    new UserRoleHistoryDto
                    {
                        Id = 4,
                        RoleName = "Trainee",
                        RoleDescription = "Thực tập sinh phát triển phần mềm",
                        StartDate = DateTime.Now.AddYears(-3).AddMonths(-6),
                        EndDate = DateTime.Now.AddYears(-3),
                        AssignedBy = "HR Department",
                        IsCurrent = false,
                        Status = "Đã hoàn thành"
                    }
                }
            };
        }
    }
}
