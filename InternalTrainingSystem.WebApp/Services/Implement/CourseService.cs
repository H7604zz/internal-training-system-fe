using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using System.Text;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CourseService> _logger;

        public CourseService(HttpClient httpClient, ILogger<CourseService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CourseDto>> GetAllCoursesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/Course"));

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var courses = JsonSerializer.Deserialize<List<CourseDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return courses ?? new List<CourseDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get courses. Status: {response.StatusCode}");
                    return new List<CourseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses");
                return new List<CourseDto>();
            }
        }

        public async Task<List<CourseDto>> GetCoursesByIdentifiersAsync(List<string> identifiers)
        {
            try
            {
                var request = new GetCoursesByIdentifiersRequest { Identifiers = identifiers };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/Course/by-identifiers"), content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var courses = JsonSerializer.Deserialize<List<CourseDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return courses ?? new List<CourseDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get courses by identifiers. Status: {response.StatusCode}");
                    return new List<CourseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by identifiers");
                return new List<CourseDto>();
            }
        }

        public async Task<CourseDetailDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/Course/{courseId}"));

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var course = JsonSerializer.Deserialize<CourseDetailDto>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return course;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    _logger.LogError($"Failed to get course by id {courseId}. Status: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting course by id {courseId}");
                return null;
            }
        }
    }
}
