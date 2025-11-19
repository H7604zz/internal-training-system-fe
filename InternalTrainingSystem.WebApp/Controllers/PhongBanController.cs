using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;
using System.Text;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("phong-ban")]
    public class PhongBanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PhongBanController(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet()]
        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/department"));

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách phòng ban.";
                    return View(new List<DepartmnentViewDto>());
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var departments = JsonSerializer.Deserialize<List<DepartmnentViewDto>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<DepartmnentViewDto>();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    departments = departments
                        .Where(d => d.DepartmentName != null && 
                                   d.DepartmentName.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                ViewBag.CurrentSearch = search;
                return View(departments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<DepartmnentViewDto>());
            }
        }
        
        
    }
}
