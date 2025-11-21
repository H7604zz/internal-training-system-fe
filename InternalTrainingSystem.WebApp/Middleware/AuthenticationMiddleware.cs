using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Tạo IHttpContextAccessor wrapper cho context hiện tại
            var httpContextAccessor = new HttpContextAccessorWrapper(context);

            // Kiểm tra nếu đây là request đến trang đăng nhập hoặc các trang public
            var path = context.Request.Path.Value?.ToLower();
            var isPublicPath = IsPublicPath(path);

            _logger.LogInformation("AuthMiddleware - Path: {Path}, IsPublic: {IsPublic}", path, isPublicPath);

            var accessToken = TokenHelpers.GetAccessToken(httpContextAccessor);
            _logger.LogInformation("AuthMiddleware - HasToken: {HasToken}", !string.IsNullOrEmpty(accessToken));
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                var isTokenExpired = TokenHelpers.IsTokenExpired(accessToken);
                _logger.LogInformation("AuthMiddleware - TokenExpired: {IsExpired}", isTokenExpired);

                if (isTokenExpired)
                    {
                        // Thử refresh token
                        var refreshToken = TokenHelpers.GetRefreshToken(httpContextAccessor);
                        _logger.LogInformation("Refresh token exists: {HasRefreshToken}", !string.IsNullOrEmpty(refreshToken));

                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            try
                            {
                                var refreshResult = await RefreshTokenAsync(refreshToken, httpContextAccessor);
                                _logger.LogInformation("Refresh token result: {Success}", refreshResult.Success);

                                if (!refreshResult.Success)
                                {
                                    // Refresh thất bại, clear tokens và để Authorization attribute handle redirect
                                    TokenHelpers.ClearTokens(httpContextAccessor);
                                    _logger.LogWarning("Refresh failed, clearing tokens");
                                }
                                else
                                {
                                    // Lấy lại token mới sau khi refresh
                                    accessToken = TokenHelpers.GetAccessToken(httpContextAccessor);
                                    _logger.LogInformation("Token refreshed successfully");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi refresh token");
                                TokenHelpers.ClearTokens(httpContextAccessor);
                            }
                        }
                        else
                        {
                            // Không có refresh token, clear tokens
                            TokenHelpers.ClearTokens(httpContextAccessor);
                            _logger.LogWarning("No refresh token, clearing tokens");
                        }
                    }

                    // Tạo claims principal từ token hợp lệ
                    if (!string.IsNullOrEmpty(accessToken) && !TokenHelpers.IsTokenExpired(accessToken))
                    {
                        try
                        {
                            var claimsPrincipal = CreateClaimsPrincipalFromToken(accessToken, context);
                            if (claimsPrincipal != null)
                            {
                                // Set user principal cho request hiện tại
                                context.User = claimsPrincipal;
                                _logger.LogInformation("AuthMiddleware - User set! Name: {UserName}, Role: {Role}, IsAuth: {IsAuth}", 
                                    claimsPrincipal.Identity?.Name, 
                                    claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value,
                                    claimsPrincipal.Identity?.IsAuthenticated);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to create claims principal from token");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi tạo claims principal từ token");
                        }
                    }
            }
            else
            {
                _logger.LogInformation("AuthMiddleware - No token found for path: {Path}", path);
            }

            await _next(context);
        }

        private async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var refreshRequest = new { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(refreshRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/auth/refresh-token"), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (refreshResponse != null && refreshResponse.Success)
                    {
                        TokenHelpers.SaveTokensToSession(httpContextAccessor, refreshResponse.AccessToken, refreshResponse.RefreshToken);
                        TokenHelpers.ExtractAndSaveUserInfo(httpContextAccessor, refreshResponse.AccessToken);
                        return new RefreshTokenResult { Success = true };
                    }

                    return new RefreshTokenResult { Success = false, Message = "Không thể làm mới token" };
                }
                else
                {
                    return new RefreshTokenResult { Success = false, Message = "Token đã hết hạn" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token in middleware");
                return new RefreshTokenResult { Success = false, Message = "Đã xảy ra lỗi khi làm mới token" };
            }
        }

        private static bool IsPublicPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var publicPaths = new[]
            {
                "/dang-nhap",
                "/quen-mat-khau",
                "/khong-co-quyen",
                "/trang-chu",
                "/css/",
                "/js/",
                "/images/",
                "/lib/",
                "/favicon.ico",
            };

            // Special case for root path
            if (path == "/" || path == "/trang-chu" || path == "/dang-nhap" || path == "/quen-mat-khau" || path == "/khong-co-quyen")
                return true;

            var isPublic = publicPaths.Any(publicPath => path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase));
            
            return isPublic;
        }

        private static bool RequiresAuthentication(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Các đường dẫn yêu cầu xác thực
            var protectedPaths = new[]
            {
                "/auth/doi-mat-khau",
                "/admin/",
                "/manager/",
                "/user/",
                "/profile/",
                "/course/",
                "/bai-thi/",
                "/khoa-hoc/danh-sach-khoa-hoc-cua-toi"
            };

            return protectedPaths.Any(protectedPath => path.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase));
        }

        private ClaimsPrincipal? CreateClaimsPrincipalFromToken(string token, HttpContext context)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                var claims = new List<Claim>();

                // Lấy thông tin từ JWT token
                foreach (var claim in jsonToken.Claims)
                {
                    switch (claim.Type)
                    {
                        case "nameid":
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                            break;
                        case "unique_name":
                            claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                            break;
                        case "email":
                            claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                            break;
                        case "role":
                            claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                            break;
                        default:
                            claims.Add(claim);
                            break;
                    }
                }

                // Lấy thông tin từ session nếu có
                var userId = context.Session.GetString("Id");
                var userRole = context.Session.GetString("Role");
                var userEmail = context.Session.GetString("Email");
                var userName = context.Session.GetString("Name");

                // Thêm claims từ session nếu chưa có
                if (!string.IsNullOrEmpty(userId) && !claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                }

                if (!string.IsNullOrEmpty(userRole) && !claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                if (!string.IsNullOrEmpty(userEmail) && !claims.Any(c => c.Type == ClaimTypes.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, userEmail));
                }

                if (!string.IsNullOrEmpty(userName) && !claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, userName));
                }

                // IMPORTANT: Use "CustomCookie" as authenticationType AND set IsAuthenticated
                var identity = new ClaimsIdentity(claims, "CustomCookie", ClaimTypes.Name, ClaimTypes.Role);
                var principal = new ClaimsPrincipal(identity);
                
                _logger.LogInformation("Created ClaimsPrincipal with {ClaimCount} claims. IsAuthenticated: {IsAuth}, Role: {Role}", 
                    claims.Count, 
                    identity.IsAuthenticated,
                    claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "none");
                
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi parse JWT token để tạo claims principal");
                return null;
            }
        }
    }

    // Helper classes for middleware
    public class HttpContextAccessorWrapper : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }

        public HttpContextAccessorWrapper(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }
    }

    public class RefreshTokenResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}