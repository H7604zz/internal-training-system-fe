using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<StaffConfirmCourseResponse>> GetUserRoleStaffConfirmCourse(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/user/{courseId}/confirmed-staff");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<StaffConfirmCourseResponse>>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new List<StaffConfirmCourseResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting staff confirmation for course {CourseId}", courseId);
                return new List<StaffConfirmCourseResponse>();
            }
        }

        public async Task<List<EligibleStaffResponse>> GetUserRoleEligibleStaff(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/user/{courseId}/eligible-staff");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<EligibleStaffResponse>>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new List<EligibleStaffResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting staff confirmation for course {CourseId}", courseId);
                return new List<EligibleStaffResponse>();
            }
        }

        public async Task<List<MentorResponse>> GetMentors()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/mentors"));
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<MentorResponse>>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new List<MentorResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting mentors");
                return new List<MentorResponse>();
            }
        }

        public async Task<List<StaffResponse>> GetAllStaff()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/staff"));
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<StaffResponse>>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new List<StaffResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting all staff");
                return new List<StaffResponse>();
            }
        }
    }
}
