using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;
using System.Text;

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

            if (!isPublicPath)
            {
                // Kiểm tra token và refresh nếu cần
                var accessToken = TokenHelpers.GetAccessToken(httpContextAccessor);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    if (TokenHelpers.IsTokenExpired(accessToken))
                    {
                        // Thử refresh token
                        var refreshToken = TokenHelpers.GetRefreshToken(httpContextAccessor);
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            try
                            {
                                var refreshResult = await RefreshTokenAsync(refreshToken, httpContextAccessor);
                                if (!refreshResult.Success)
                                {
                                    // Refresh thất bại, chuyển hướng đến trang đăng nhập
                                    TokenHelpers.ClearTokens(httpContextAccessor);
                                    context.Response.Redirect("/Auth/dang-nhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi refresh token");
                                TokenHelpers.ClearTokens(httpContextAccessor);
                                context.Response.Redirect("/Auth/dang-nhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                                return;
                            }
                        }
                        else
                        {
                            // Không có refresh token, chuyển hướng đến trang đăng nhập
                            TokenHelpers.ClearTokens(httpContextAccessor);
                            context.Response.Redirect("/Auth/dang-nhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                            return;
                        }
                    }
                }
                else
                {
                    // Không có access token, chuyển hướng đến trang đăng nhập cho các trang yêu cầu xác thực
                    if (RequiresAuthentication(path))
                    {
                        context.Response.Redirect("/Auth/dang-nhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                        return;
                    }
                }
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
                "/auth/dang-nhap",
                "/auth/quen-mat-khau",
                "/home/index",
                "/",
                "/css/",
                "/js/",
                "/images/",
                "/lib/",
                "/favicon.ico"
            };

            return publicPaths.Any(publicPath => path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase));
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
                "/quiz/"
            };

            return protectedPaths.Any(protectedPath => path.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase));
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