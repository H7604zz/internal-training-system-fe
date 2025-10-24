using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using System.Net;
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

        public async Task<PagedResult<CourseDto>> GetCoursesAsync(string? search = null, string? status = null, int page = 1, int pageSize = 10)
        {
            try
            {
                // Xây dựng query string với các tham số
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                }

                if (!string.IsNullOrEmpty(status))
                {
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                }

                var queryString = string.Join("&", queryParams);
                var url = Utilities.GetAbsoluteUrl($"api/course?{queryString}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return new PagedResult<CourseDto>();
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // API trả về PagedResult<CourseDto>
                    var pagedResult = JsonSerializer.Deserialize<PagedResult<CourseDto>>(jsonString, options);

                    return pagedResult ?? new PagedResult<CourseDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get courses. Status: {response.StatusCode}");
                    return new PagedResult<CourseDto>();
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error. The API response format may not match the expected CourseDto list structure.");
                return new PagedResult<CourseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses");
                return new PagedResult<CourseDto>();
            }
        }

        public async Task<List<CourseDto>> GetCoursesByIdentifiersAsync(List<string> identifiers)
        {
            try
            {
                var request = new GetCoursesByIdentifiersRequest { Identifiers = identifiers };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/course/by-identifiers"), content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var courses = JsonSerializer.Deserialize<List<CourseDto>>(jsonString, options);
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

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                // Sử dụng endpoint detail của API
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{courseId}/detail"));

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var course = JsonSerializer.Deserialize<CourseDto>(jsonString, options);
                    return course;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
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

        public async Task<CourseDto?> CreateCourseAsync(CourseDto course)
        {
            try
            {
                var json = JsonSerializer.Serialize(course);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/course"), content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var createdCourse = JsonSerializer.Deserialize<CourseDto>(jsonString, options);
                    return createdCourse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create course. Status: {response.StatusCode}, Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating course");
                return null;
            }
        }

        public async Task<CourseDto?> UpdateCourseAsync(CourseDto course)
        {
            try
            {
                var json = JsonSerializer.Serialize(course);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(Utilities.GetAbsoluteUrl($"api/course/{course.CourseId}"), content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var updatedCourse = JsonSerializer.Deserialize<CourseDto>(jsonString, options);
                    return updatedCourse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to update course {course.CourseId}. Status: {response.StatusCode}, Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating course {course.CourseId}");
                return null;
            }
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(Utilities.GetAbsoluteUrl($"api/course/{courseId}"));

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to delete course {courseId}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting course {courseId}");
                return false;
            }
        }

        public async Task<CourseApprovalResponse> ApproveCourseAsync(CourseApprovalRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/course/approve"), content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var result = JsonSerializer.Deserialize<CourseApprovalResponse>(jsonString, options);
                    return result ?? new CourseApprovalResponse { Success = false, Message = "Unknown error occurred" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to approve course {request.CourseId}. Status: {response.StatusCode}, Error: {errorContent}");
                    
                    return new CourseApprovalResponse 
                    { 
                        Success = false, 
                        Message = "Không thể phê duyệt khóa học. Vui lòng thử lại sau.",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while approving course {request.CourseId}");
                return new CourseApprovalResponse 
                { 
                    Success = false, 
                    Message = "Đã xảy ra lỗi khi phê duyệt khóa học",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<CourseApprovalResponse> RejectCourseAsync(CourseApprovalRequest request)
        {
            try
            {   
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/course/reject"), content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var result = JsonSerializer.Deserialize<CourseApprovalResponse>(jsonString, options);
                    return result ?? new CourseApprovalResponse { Success = false, Message = "Unknown error occurred" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to reject course {request.CourseId}. Status: {response.StatusCode}, Error: {errorContent}");
                    
                    return new CourseApprovalResponse 
                    { 
                        Success = false, 
                        Message = "Không thể từ chối khóa học. Vui lòng thử lại sau.",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while rejecting course {request.CourseId}");
                return new CourseApprovalResponse 
                { 
                    Success = false, 
                    Message = "Đã xảy ra lỗi khi từ chối khóa học",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<List<CourseDto>> GetPendingCoursesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/course/pending"));

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var courses = JsonSerializer.Deserialize<List<CourseDto>>(jsonString, options);
                    return courses ?? new List<CourseDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get pending courses. Status: {response.StatusCode}");
                    return new List<CourseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pending courses");
                return new List<CourseDto>();
            }
        }

        public async Task<List<EmployeeCourseDto>> GetEmployeeCoursesAsync(string employeeId)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/employee/{employeeId}"));

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var courses = JsonSerializer.Deserialize<List<EmployeeCourseDto>>(jsonString, options);
                    return courses ?? new List<EmployeeCourseDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get employee courses for {employeeId}. Status: {response.StatusCode}");
                    return new List<EmployeeCourseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting employee courses for {employeeId}");
                return new List<EmployeeCourseDto>();
            }
        }

        public async Task<bool> RespondToCourseAsync(int courseId, string employeeId, string responseType, string? note = null)
        {
            try
            {
                var request = new
                {
                    CourseId = courseId,
                    EmployeeId = employeeId,
                    ResponseType = responseType,
                    Note = note
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/course/respond"), content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to respond to course {courseId} for employee {employeeId}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while responding to course {courseId} for employee {employeeId}");
                return false;
            }
        }
    }
}
