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

        [HttpGet("chi-tiet")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/department/{id}"));

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Không tìm thấy phòng ban.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết phòng ban.";
                    }
                    return RedirectToAction("Index");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var dept = JsonSerializer.Deserialize<DepartmentDetailDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dept == null)
                {
                    TempData["Error"] = "Không tìm thấy phòng ban.";
                    return RedirectToAction("Index");
                }

                return View(dept);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        // GET: PhongBan/TaoMoi
        [HttpGet("tao-moi")]
        public IActionResult TaoMoi()
        {
            return View();
        }

        // POST: PhongBan/TaoMoi
        [HttpPost("tao-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoi(CreateDepartmentDto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var json = JsonSerializer.Serialize(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/department"), content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Tạo phòng ban thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"Có lỗi xảy ra: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

    }
}
