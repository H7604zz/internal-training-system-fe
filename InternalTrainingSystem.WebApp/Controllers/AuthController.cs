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
