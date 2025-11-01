using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;
using System.Text;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Trang đăng nhập - URL sẽ là /dang-nhap
        /// </summary>
        [HttpGet("/dang-nhap")]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            // Kiểm tra nếu user đã đăng nhập
            if (!TokenHelpers.IsTokenExpired(_httpContextAccessor))
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
        [HttpPost("/dang-nhap")]
        public async Task<IActionResult> DangNhap(LoginRequestDto model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                var result = await LoginAsync(model);

                if (result.Success)
                {
                    // Lưu thông tin user vào session nếu cần
                    if (result.User != null)
                    {
                        Extensions.SessionExtensions.SetUserInfo(HttpContext.Session, result.User.FullName, result.User.Email, result.User.Id);
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
        /// Trang quên mật khẩu - URL sẽ là /quen-mat-khau
        /// </summary>
        [HttpGet("/quen-mat-khau")]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        /// <summary>
        /// Xử lý quên mật khẩu
        /// </summary>
        [HttpPost("/quen-mat-khau")]
        public async Task<IActionResult> QuenMatKhau(ForgotPasswordRequestDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await ForgotPasswordAsync(model);

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
        [HttpGet("/dang-xuat")]
        public async Task<IActionResult> DangXuat()
        {
            try
            {
                await LogoutAsync();
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
        /// Trang thông báo không có quyền truy cập
        /// </summary>
        [HttpGet("/khong-co-quyen")]
        public IActionResult KhongCoQuyen()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu - Sử dụng AuthService với bool return
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(ChangePasswordRequestDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Nếu là AJAX request, trả về JSON
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                        return Json(new { success = false, message = string.Join(", ", errors) });
                    }
                    
                    return View(model);
                }

                // Sử dụng AuthService để đổi mật khẩu
                var isSuccess = await ChangePasswordAsync(model);

                if (isSuccess)
                {
                    // Nếu là AJAX request, trả về JSON
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Đổi mật khẩu thành công" });
                    }
                    
                    TempData["Success"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("ThongTinCaNhan");
                }
                else
                {
                    // Lấy error message từ API
                    var errorMessage = await GetChangePasswordErrorAsync(model);
                    
                    // Nếu là AJAX request, trả về JSON với error message trong popup
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                    
                    TempData["Error"] = errorMessage;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Đã xảy ra lỗi trong quá trình đổi mật khẩu. Vui lòng thử lại.";
                
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["Error"] = errorMessage;
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
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap");
                }

                // Lấy thông tin user từ API
                var userProfile = await GetProfileAsync();
                
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
                    var userProfile = await GetProfileAsync();
                    if (userProfile != null)
                    {
                        ViewBag.ShowUpdateForm = true;
                        return View("ThongTinCaNhan", userProfile);
                    }

                    TempData["Error"] = "Không thể lấy thông tin cá nhân để hiển thị.";
                    return RedirectToAction("ThongTinCaNhan");
                }

                // Gọi API update
                var message = await UpdateProfileAsync(model);

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
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var userProfile = await GetProfileAsync();
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

        /// <summary>
        /// API endpoint gửi OTP cho quên mật khẩu
        /// </summary>
        [HttpPost]
        [Route("api/auth/forgot-password")]
        public async Task<IActionResult> SendOtp([FromBody] ForgotPasswordRequestDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Email không hợp lệ" });
                }

                var result = await ForgotPasswordAsync(model);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi OTP" });
            }
        }

        /// <summary>
        /// API endpoint xác minh OTP và đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("api/auth/reset-password")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordRequestDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var result = await ResetPasswordAsync(model);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi đặt lại mật khẩu" });
            }
        }

        

        

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/login"), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResponse != null && loginResponse.Success)
                    {
                        // Lưu token vào session hoặc cookie
                        TokenHelpers.SaveTokensToSession(_httpContextAccessor, loginResponse.AccessToken, loginResponse.RefreshToken);
                    }

                    return loginResponse ?? new LoginResponseDto { Success = false, Message = "Đã xảy ra lỗi khi xử lý phản hồi" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new LoginResponseDto { Success = false, Message = "Đăng nhập thất bại" };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Không thể kết nối đến máy chủ. Vui lòng thử lại sau."
                };
            }
        }

        public async Task<LogoutResponseDto> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/logout"), null);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Xóa token khỏi session dù API có thành công hay không
                TokenHelpers.ClearTokens(_httpContextAccessor);
                Extensions.SessionExtensions.ClearUserInfo(HttpContext.Session);
                if (response.IsSuccessStatusCode)
                {
                    var logoutResponse = JsonSerializer.Deserialize<LogoutResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return logoutResponse ?? new LogoutResponseDto { Success = true, Message = "Đăng xuất thành công" };
                }
                else
                {
                    // Vẫn trả về thành công vì đã xóa token local
                    return new LogoutResponseDto { Success = true, Message = "Đăng xuất thành công" };
                }
            }
            catch (Exception ex)
            {
                // Vẫn xóa token local
                TokenHelpers.ClearTokens(_httpContextAccessor);
                return new LogoutResponseDto { Success = true, Message = "Đăng xuất thành công" };
            }
        }

        public async Task<UserProfileDto?> GetProfileAsync()
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/profile"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var profileResponse = JsonSerializer.Deserialize<UserProfileDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return profileResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token có thể đã hết hạn, thử refresh
                    var refreshResult = await TryRefreshToken();
                    if (refreshResult)
                    {
                        // Thử lại với token mới
                        return await GetProfileAsync();
                    }

                    TokenHelpers.ClearTokens(_httpContextAccessor);
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest)
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var json = JsonSerializer.Serialize(changePasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/change-password"), content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> GetChangePasswordErrorAsync(ChangePasswordRequestDto changePasswordRequest)
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return "Không tìm thấy token xác thực";
                }

                var json = JsonSerializer.Serialize(changePasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/change-password"), content);
                var message = await response.Content.ReadAsStringAsync();

                // Trim quotes from response if it's a plain string
                message = message.Trim('"');

                if (!response.IsSuccessStatusCode)
                {
                    return string.IsNullOrEmpty(message) ? "Đổi mật khẩu thất bại" : message;
                }

                return string.Empty; // Success case
            }
            catch (Exception ex)
            {
                return "Đã xảy ra lỗi khi đổi mật khẩu";
            }
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var refreshRequest = new { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(refreshRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/refresh-token"), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var refreshResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (refreshResponse != null && refreshResponse.Success)
                    {
                        TokenHelpers.SaveTokensToSession(_httpContextAccessor, refreshResponse.AccessToken, refreshResponse.RefreshToken);
                    }

                    return refreshResponse ?? new LoginResponseDto { Success = false, Message = "Không thể làm mới token" };
                }
                else
                {
                    return new LoginResponseDto { Success = false, Message = "Token đã hết hạn" };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponseDto { Success = false, Message = "Đã xảy ra lỗi khi làm mới token" };
            }
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(forgotPasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/forgot-password"), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var forgotPasswordResponse = JsonSerializer.Deserialize<ForgotPasswordResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return forgotPasswordResponse ?? new ForgotPasswordResponseDto { Success = true, Message = "Đã gửi email đặt lại mật khẩu" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ForgotPasswordResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new ForgotPasswordResponseDto { Success = false, Message = "Không thể gửi email đặt lại mật khẩu" };
                }
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponseDto { Success = false, Message = "Đã xảy ra lỗi khi gửi email đặt lại mật khẩu" };
            }
        }

        public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto verifyOtpRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(verifyOtpRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/verify-otp"), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var verifyOtpResponse = JsonSerializer.Deserialize<VerifyOtpResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return verifyOtpResponse ?? new VerifyOtpResponseDto { Success = true, Message = "Xác minh OTP thành công" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<VerifyOtpResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new VerifyOtpResponseDto { Success = false, Message = "Xác minh OTP thất bại" };
                }
            }
            catch (Exception ex)
            {
                return new VerifyOtpResponseDto { Success = false, Message = "Đã xảy ra lỗi khi xác minh OTP" };
            }
        }

        public async Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(resetPasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/reset-password"), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var resetPasswordResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return resetPasswordResponse ?? new ApiResponseDto { Success = true, Message = "Đặt lại mật khẩu thành công" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new ApiResponseDto { Success = false, Message = "Đặt lại mật khẩu thất bại" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponseDto { Success = false, Message = "Đã xảy ra lỗi khi đặt lại mật khẩu" };
            }
        }

        private async Task<bool> TryRefreshToken()
        {
            var refreshToken = TokenHelpers.GetRefreshToken(_httpContextAccessor);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var refreshResult = await RefreshTokenAsync(refreshToken);
            return refreshResult.Success;
        }

        public async Task<string> UpdateProfileAsync(UpdateProfileDto model)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PatchAsync(Utilities.GetAbsoluteUrl("api/user/update-profile"), jsonContent);

            var message = await response.Content.ReadAsStringAsync();
            message = message.Trim('"');

            if (response.IsSuccessStatusCode)
            {
                return message;
            }
            else
            {
                throw new Exception(message);
            }
        }
    }
}
