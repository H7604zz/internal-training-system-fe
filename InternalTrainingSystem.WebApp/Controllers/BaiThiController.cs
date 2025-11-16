using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("quiz")]
    public class BaiThiController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BaiThiController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaiThiController(
            IHttpClientFactory httpClientFactory,
            ILogger<BaiThiController> logger,
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

        // API để lấy thông tin tổng quan quiz
        [HttpGet("get-quiz-info/{quizId}")]
        public async Task<IActionResult> GetQuizInfo(int quizId)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/{quizId}/info");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {errorContent}");
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể lấy thông tin quiz. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetQuizInfo: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // API để lấy lịch sử làm quiz
        [HttpGet("get-quiz-history/{courseId}/{quizId}")]
        public async Task<IActionResult> GetQuizHistory(int courseId, int quizId)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/{courseId}/{quizId}/history");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {errorContent}");
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể lấy lịch sử quiz. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetQuizHistory: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}