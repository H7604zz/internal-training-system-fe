using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KhoaHocController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizController(
            IHttpClientFactory httpClientFactory,
            ILogger<KhoaHocController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        
    }
}