using InternalTrainingSystem.WebApp.Models.DTOs;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Services.Interface;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
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
                        SaveTokensToSession(loginResponse.AccessToken, loginResponse.RefreshToken);
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
                _logger.LogError(ex, "Lỗi khi gọi API đăng nhập");
                return new LoginResponseDto 
                { 
                    Success = false, 
                    Message = "Không thể kết nối đến máy chủ. Vui lòng thử lại sau." 
                };
            }
        }

        public async Task<ApiResponseDto> LogoutAsync()
        {
            try
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync("/api/auth/logout", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Xóa token khỏi session dù API có thành công hay không
                ClearTokens();

                if (response.IsSuccessStatusCode)
                {
                    var logoutResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return logoutResponse ?? new ApiResponseDto { Success = true, Message = "Đăng xuất thành công" };
                }
                else
                {
                    // Vẫn trả về thành công vì đã xóa token local
                    return new ApiResponseDto { Success = true, Message = "Đăng xuất thành công" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API đăng xuất");
                // Vẫn xóa token local
                ClearTokens();
                return new ApiResponseDto { Success = true, Message = "Đăng xuất thành công" };
            }
        }

        public async Task<ApiResponseDto> GetProfileAsync()
        {
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new ApiResponseDto { Success = false, Message = "Không tìm thấy token xác thực" };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/auth/profile"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var profileResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return profileResponse ?? new ApiResponseDto { Success = false, Message = "Không thể lấy thông tin người dùng" };
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
                    
                    ClearTokens();
                    return new ApiResponseDto { Success = false, Message = "Phiên đăng nhập đã hết hạn" };
                }
                else
                {
                    return new ApiResponseDto { Success = false, Message = "Không thể lấy thông tin người dùng" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API lấy thông tin người dùng");
                return new ApiResponseDto { Success = false, Message = "Đã xảy ra lỗi khi lấy thông tin người dùng" };
            }
        }

        public async Task<ApiResponseDto> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest)
        {
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new ApiResponseDto { Success = false, Message = "Không tìm thấy token xác thực" };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(changePasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/change-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var changePasswordResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return changePasswordResponse ?? new ApiResponseDto { Success = true, Message = "Đổi mật khẩu thành công" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new ApiResponseDto { Success = false, Message = "Đổi mật khẩu thất bại" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API đổi mật khẩu");
                return new ApiResponseDto { Success = false, Message = "Đã xảy ra lỗi khi đổi mật khẩu" };
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
                        SaveTokensToSession(refreshResponse.AccessToken, refreshResponse.RefreshToken);
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
                _logger.LogError(ex, "Lỗi khi làm mới token");
                return new LoginResponseDto { Success = false, Message = "Đã xảy ra lỗi khi làm mới token" };
            }
        }

        public async Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(forgotPasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/forgot-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var forgotPasswordResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return forgotPasswordResponse ?? new ApiResponseDto { Success = true, Message = "Đã gửi email đặt lại mật khẩu" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new ApiResponseDto { Success = false, Message = "Không thể gửi email đặt lại mật khẩu" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API quên mật khẩu");
                return new ApiResponseDto { Success = false, Message = "Đã xảy ra lỗi khi gửi email đặt lại mật khẩu" };
            }
        }

        public bool IsTokenExpired()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }

            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwtToken.ValidTo <= DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        public void ClearTokens()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove("AccessToken");
                session.Remove("RefreshToken");
            }
        }

        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        }

        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("RefreshToken");
        }

        private void SaveTokensToSession(string accessToken, string refreshToken)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString("AccessToken", accessToken);
                session.SetString("RefreshToken", refreshToken);
            }
        }

        private async Task<bool> TryRefreshToken()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var refreshResult = await RefreshTokenAsync(refreshToken);
            return refreshResult.Success;
        }
    }
}