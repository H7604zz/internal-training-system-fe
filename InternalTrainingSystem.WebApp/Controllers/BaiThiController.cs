using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models.DTOs;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("bai-thi")]
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

        // View trang làm bài
        [HttpGet("lam-bai/{quizId}")]
        public IActionResult LamBai(int quizId)
        {
            ViewBag.QuizId = quizId;
            return View();
        }

        // API bắt đầu làm quiz (start attempt)
        [HttpPost("start-quiz/{lessonId}")]
        public async Task<IActionResult> StartQuiz(int lessonId)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/start/lesson/{lessonId}");
                var response = await _httpClient.PostAsync(apiUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {errorContent}");
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể bắt đầu làm quiz. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in StartQuiz: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // API lấy quiz cho attempt (để làm bài)
        [HttpGet("get-quiz-for-attempt/{quizId}/{attemptId}")]
        public async Task<IActionResult> GetQuizForAttempt(int quizId, int attemptId, [FromQuery] bool shuffleQuestions = true, [FromQuery] bool shuffleAnswers = true)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/{quizId}/attempt/{attemptId}?shuffleQuestions={shuffleQuestions}&shuffleAnswers={shuffleAnswers}");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {errorContent}");
                    
                    // Handle special case: quiz timeout
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return StatusCode(409, new { code = "QUIZ_TIMED_OUT", message = errorContent });
                    }
                    
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể lấy thông tin quiz. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetQuizForAttempt: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // API nộp bài quiz
        [HttpPost("submit-quiz/{lessonId}/{attemptId}")]
        public async Task<IActionResult> SubmitQuiz(int lessonId, int attemptId, [FromBody] SubmitAttemptRequest request)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/submit/lesson/{lessonId}/attempt/{attemptId}");
                var jsonContent = JsonSerializer.Serialize(request);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(apiUrl, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {errorContent}");
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể nộp bài. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SubmitQuiz: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // View trang kết quả
        [HttpGet("ket-qua/{attemptId}")]
        public IActionResult KetQua(int attemptId)
        {
            ViewBag.AttemptId = attemptId;
            return View();
        }

        // API lấy kết quả attempt
        [HttpGet("get-result/{attemptId}")]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            try
            {
                var apiUrl = Utilities.GetAbsoluteUrl($"api/quiz/attempt/{attemptId}/result");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {errorContent}");
                    return StatusCode((int)response.StatusCode, new { message = $"Không thể lấy kết quả. Status: {response.StatusCode}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetResult: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}