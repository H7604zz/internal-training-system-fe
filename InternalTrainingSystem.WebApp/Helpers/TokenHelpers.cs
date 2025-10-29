using System;
using System.IdentityModel.Tokens.Jwt;
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
    }
}
