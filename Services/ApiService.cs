using InternalTrainingSystem.WebApp.Models;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services
{
    public interface IApiService
    {
        Task<List<User>> GetUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<List<Course>> GetCoursesAsync();
        Task<Course?> GetCourseByIdAsync(int id);
        Task<List<Enrollment>> GetEnrollmentsAsync();
        Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(int userId);
        Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<List<Material>> GetMaterialsByCourseIdAsync(int courseId);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions);
                
                return users ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users from API");
                return new List<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{id}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(json, _jsonOptions);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId} from API", id);
                return null;
            }
        }

        public async Task<List<Course>> GetCoursesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/courses");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<Course>>(json, _jsonOptions);
                
                return courses ?? new List<Course>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching courses from API");
                return new List<Course>();
            }
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/courses/{id}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<Course>(json, _jsonOptions);
                
                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching course {CourseId} from API", id);
                return null;
            }
        }

        public async Task<List<Enrollment>> GetEnrollmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/enrollments");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var enrollments = JsonSerializer.Deserialize<List<Enrollment>>(json, _jsonOptions);
                
                return enrollments ?? new List<Enrollment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching enrollments from API");
                return new List<Enrollment>();
            }
        }

        public async Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/enrollments/user/{userId}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var enrollments = JsonSerializer.Deserialize<List<Enrollment>>(json, _jsonOptions);
                
                return enrollments ?? new List<Enrollment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching enrollments for user {UserId} from API", userId);
                return new List<Enrollment>();
            }
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/enrollments/course/{courseId}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var enrollments = JsonSerializer.Deserialize<List<Enrollment>>(json, _jsonOptions);
                
                return enrollments ?? new List<Enrollment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching enrollments for course {CourseId} from API", courseId);
                return new List<Enrollment>();
            }
        }

        public async Task<List<Material>> GetMaterialsByCourseIdAsync(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/materials/course/{courseId}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var materials = JsonSerializer.Deserialize<List<Material>>(json, _jsonOptions);
                
                return materials ?? new List<Material>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching materials for course {CourseId} from API", courseId);
                return new List<Material>();
            }
        }
    }
}