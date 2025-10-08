using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using System.Text;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class ClassService : IClassService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClassService> _logger;

        public ClassService(
            HttpClient httpClient,
            ILogger<ClassService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ClassDto>> GetClassesAsync()
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl("api/Class");
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var classes = JsonSerializer.Deserialize<List<ClassDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return classes ?? new List<ClassDto>();
                }
                else
                {
                    _logger.LogError("Failed to get classes. Status: {StatusCode}", response.StatusCode);
                    return new List<ClassDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classes");
                return new List<ClassDto>();
            }
        }

        public async Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl("api/Class");
                var jsonContent = JsonSerializer.Serialize(createClassesDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var classes = JsonSerializer.Deserialize<List<ClassDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return classes ?? new List<ClassDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create classes. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new Exception($"Failed to create classes: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating classes");
                throw;
            }
        }


    }
}
