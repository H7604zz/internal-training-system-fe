using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace InternalTrainingSystem.WebApp.Helpers
{
    public static class TokenHelpers
    {
        public static bool IsTokenExpired(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return true;

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

        public static string? GetAccessToken(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        }

        public static string? GetRefreshToken(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?.Session.GetString("RefreshToken");
        }

        public static bool IsTokenExpired(IHttpContextAccessor httpContextAccessor)
        {
            var token = GetAccessToken(httpContextAccessor);
            return IsTokenExpired(token);
        }

        public static void ClearTokens(IHttpContextAccessor httpContextAccessor)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove("AccessToken");
                session.Remove("RefreshToken");
            }
        }

        public static void SaveTokensToSession(IHttpContextAccessor httpContextAccessor, string accessToken, string refreshToken)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString("AccessToken", accessToken);
                session.SetString("RefreshToken", refreshToken);
            }
        }

        /// <summary>
        /// Extract user info from JWT token and save to session using SessionExtensions
        /// </summary>
        public static void ExtractAndSaveUserInfo(IHttpContextAccessor httpContextAccessor, string accessToken)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessToken);

                // Role claim có thể có nhiều format khác nhau
                var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "role" ||
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                // Name claim có thể có nhiều format khác nhau
                var nameClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "unique_name" ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

                // Email claim có thể có nhiều format khác nhau
                var emailClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "email" ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

                // User ID claim
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "nameid" ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" ||
                    c.Type == "sub");

                // Save to session using SessionExtensions
                if (nameClaim != null && emailClaim != null && userIdClaim != null)
                {
                    var role = roleClaim?.Value ?? string.Empty;
                    Extensions.SessionExtensions.SetUserInfo(session, nameClaim.Value, emailClaim.Value, userIdClaim.Value, role);
                }
            }
            catch
            {
                // If token parsing fails, continue without setting user info
            }
        }
    }
}
