using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(HttpClient httpClient, ILogger<DepartmentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            try
            {
                var url = Utilities.GetAbsoluteUrl("api/department");
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return new List<DepartmentDto>();
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var departments = JsonSerializer.Deserialize<List<DepartmentDto>>(jsonString, options);

                    return departments ?? new List<DepartmentDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get departments. Status: {response.StatusCode}");
                    return new List<DepartmentDto>();
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while getting departments");
                return new List<DepartmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting departments");
                return new List<DepartmentDto>();
            }
        }
    }
}
