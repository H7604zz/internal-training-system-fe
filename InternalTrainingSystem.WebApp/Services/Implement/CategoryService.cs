using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(HttpClient httpClient, ILogger<CategoryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var url = Utilities.GetAbsoluteUrl("api/category");
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return new List<CategoryDto>();
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var categories = JsonSerializer.Deserialize<List<CategoryDto>>(jsonString, options);

                    return categories ?? new List<CategoryDto>();
                }
                else
                {
                    _logger.LogError($"Failed to get categories. Status: {response.StatusCode}");
                    return new List<CategoryDto>();
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while getting categories");
                return new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting categories");
                return new List<CategoryDto>();
            }
        }
    }
}
