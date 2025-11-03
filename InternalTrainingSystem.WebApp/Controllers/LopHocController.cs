using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Models.ViewModels;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("lop-hoc")]
    public class LopHocController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LopHocController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LopHocController(IHttpClientFactory httpClientFactory, ILogger<LopHocController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                // Sử dụng page size cố định từ constants cho danh sách lớp học
                var pageSize = PaginationConstants.ClassPageSize;

                var queryParams = $"?page={page}&pageSize={pageSize}";
                // Gọi API để lấy danh sách lớp học
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/class{queryParams}"));

                PagedResult<ClassDto> pagedResult;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    pagedResult = JsonSerializer.Deserialize<PagedResult<ClassDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new PagedResult<ClassDto>();
                }
                else
                {
                    pagedResult = new PagedResult<ClassDto>();
                }

                var totalItems = pagedResult.TotalCount;
                var totalPages = pagedResult.TotalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;

                return View(pagedResult?.Items?.ToList() ?? new List<ClassDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting classes list");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách lớp học.";
                return View(new List<ClassDto>());
            }
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                // Gọi API để lấy chi tiết lớp học
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/class/{id}"));
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Không tìm thấy lớp học.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết khóa học.";
                    }
                    return RedirectToAction("Index");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var classDetail = JsonSerializer.Deserialize<ClassDetailDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (classDetail == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                return View(classDetail);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class detail with ID: {ClassId}", id);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết lớp học.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("create-class")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(CreateClassViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                    return RedirectToAction("Index", "KhoaHoc");
                }

                if (model.CourseId <= 0)
                {
                    TempData["Error"] = "Dữ liệu khóa học không hợp lệ";
                    return RedirectToAction("Index", "KhoaHoc");
                }

                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn";
                    return RedirectToAction("DangNhap", "Auth");
                }

                // Tạo request theo đúng format của API
                var createClassRequest = new 
                {
                    CourseId = model.CourseId,
                    NumberOfClasses = model.NumberOfClasses > 0 ? model.NumberOfClasses : 1
                };

                var json = JsonSerializer.Serialize(createClassRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Call the API to create classes
                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/class"),
                    content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Tạo lớp học thành công!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể tạo lớp học. Vui lòng thử lại.";
                    TempData["Error"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating class for course {CourseId}", model.CourseId);
                TempData["Error"] = "Có lỗi xảy ra khi tạo lớp học. Vui lòng thử lại sau!";
            }

            return RedirectToAction("DanhSachNhanVien", "KhoaHoc", new { id = model.CourseId });
        }
    }
}
