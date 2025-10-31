using InternalTrainingSystem.WebApp.Models.DTOs;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Helpers
{
    /// <summary>
    /// Helper class for user profile operations
    /// </summary>
    public static class UserProfileHelper
    {
        /// <summary>
        /// Get current user profile from API
        /// </summary>
        /// <param name="httpClient">HttpClient instance</param>
        /// <param name="httpContextAccessor">HttpContextAccessor instance</param>
        /// <returns>UserProfileDto if successful, null otherwise</returns>
        public static async Task<UserProfileDto?> GetCurrentUserProfileAsync(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/profile"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var profileResponse = JsonSerializer.Deserialize<UserProfileDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return profileResponse;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Check if current user has a specific role
        /// </summary>
        /// <param name="httpClient">HttpClient instance</param>
        /// <param name="httpContextAccessor">HttpContextAccessor instance</param>
        /// <param name="roleName">Role name to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public static async Task<bool> HasRoleAsync(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor, 
            string roleName)
        {
            var userProfile = await GetCurrentUserProfileAsync(httpClient, httpContextAccessor);
            return userProfile?.CurrentRole?.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Get current user role
        /// </summary>
        /// <param name="httpClient">HttpClient instance</param>
        /// <param name="httpContextAccessor">HttpContextAccessor instance</param>
        /// <returns>User role string if successful, null otherwise</returns>
        public static async Task<string?> GetCurrentUserRoleAsync(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor)
        {
            var userProfile = await GetCurrentUserProfileAsync(httpClient, httpContextAccessor);
            return userProfile?.CurrentRole;
        }
    }
}