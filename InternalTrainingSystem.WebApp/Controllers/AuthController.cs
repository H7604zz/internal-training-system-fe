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
                return RedirectToAction("", "TrangChu");
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
                    
                    return RedirectToAction("", "TrangChu");
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

                // Lấy thông tin user từ API
                var userProfile = await _authService.GetProfileAsync();
                
                if (userProfile != null)
                {
                    return View(userProfile);
                }
                
                // Nếu không lấy được profile, redirect về trang chủ với thông báo lỗi
                TempData["Error"] = "Không thể lấy thông tin cá nhân.";
                return RedirectToAction("Index", "TrangChu");
            }
            catch (Exception)
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
                    var userProfile = await _authService.GetProfileAsync();
                    if (userProfile != null)
                    {
                        ViewBag.ShowUpdateForm = true;
                        return View("ThongTinCaNhan", userProfile);
                    }

                    TempData["Error"] = "Không thể lấy thông tin cá nhân để hiển thị.";
                    return RedirectToAction("ThongTinCaNhan");
                }

                // Gọi API update
                var message = await _authService.UpdateProfileAsync(model);

                TempData["Success"] = message; 
                return RedirectToAction("ThongTinCaNhan");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message; 
                return RedirectToAction("ThongTinCaNhan");
            }
        }


        /// <summary>
        /// API endpoint để lấy thông tin người dùng hiện tại
        /// </summary>
        [HttpGet]
        [Route("api/user/profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                if (_authService.IsTokenExpired())
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var userProfile = await _authService.GetProfileAsync();
                if (userProfile != null)
                {
                    return Json(new { success = true, data = userProfile });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể lấy thông tin người dùng" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin người dùng" });
            }
        }
    }
}
