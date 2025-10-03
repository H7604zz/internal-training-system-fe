using InternalTrainingSystem.WebApp.Services.Interface;

namespace InternalTrainingSystem.WebApp.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Kiểm tra nếu đây là request đến trang đăng nhập hoặc các trang public
            var path = context.Request.Path.Value?.ToLower();
            var isPublicPath = IsPublicPath(path);

            if (!isPublicPath)
            {
                // Kiểm tra token và refresh nếu cần
                var accessToken = authService.GetAccessToken();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    if (authService.IsTokenExpired())
                    {
                        // Thử refresh token
                        var refreshToken = authService.GetRefreshToken();
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            try
                            {
                                var refreshResult = await authService.RefreshTokenAsync(refreshToken);
                                if (!refreshResult.Success)
                                {
                                    // Refresh thất bại, chuyển hướng đến trang đăng nhập
                                    authService.ClearTokens();
                                    context.Response.Redirect("/Auth/DangNhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi refresh token");
                                authService.ClearTokens();
                                context.Response.Redirect("/Auth/DangNhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                                return;
                            }
                        }
                        else
                        {
                            // Không có refresh token, chuyển hướng đến trang đăng nhập
                            authService.ClearTokens();
                            context.Response.Redirect("/Auth/DangNhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                            return;
                        }
                    }
                }
                else
                {
                    // Không có access token, chuyển hướng đến trang đăng nhập cho các trang yêu cầu xác thực
                    if (RequiresAuthentication(path))
                    {
                        context.Response.Redirect("/Auth/DangNhap?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                        return;
                    }
                }
            }

            await _next(context);
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
}