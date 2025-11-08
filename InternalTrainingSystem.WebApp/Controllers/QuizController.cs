using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("quiz")]
    public class QuizController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<QuizController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizController(
            IHttpClientFactory httpClientFactory,
            ILogger<QuizController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // API để lấy chi tiết quiz
        [HttpGet("get-quiz-detail/{quizId}")]
        public async Task<IActionResult> GetQuizDetail(int quizId)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/{quizId}");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể lấy thông tin quiz. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var quizDetail = JsonSerializer.Deserialize<QuizDetailDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (quizDetail == null)
                {
                    return StatusCode(500, new { message = "Không thể xử lý dữ liệu quiz" });
                }

                return Json(quizDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Có lỗi xảy ra khi lấy thông tin quiz: {ex.Message}" });
            }
        }
    }
}