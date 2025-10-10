using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
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
                return RedirectToAction("TrangChu", "Home");
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
                    _logger.LogInformation("Người dùng {Email} đã đăng nhập thành công", model.Email);
                    
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
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = result.Message;
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
            }
            catch (Exception ex)
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
                    TempData["SuccessMessage"] = "Nếu email này tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn.";
                    return View();
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý quên mật khẩu");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại.");
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
                _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng");
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
                _logger.LogInformation("Người dùng đã đăng xuất");
                
                // Xóa thông tin user khỏi session
                HttpContext.Session.Clear();
                
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng xuất");
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
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("DoiMatKhau");
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    if (result.Errors != null && result.Errors.Any())
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đổi mật khẩu. Vui lòng thử lại.");
                return View(model);
            }
        }
    }
}
