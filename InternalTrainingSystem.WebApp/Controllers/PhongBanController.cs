using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Helpers;
using System.Net.Http;
using InternalTrainingSystem.WebApp.Constants;
using System.Text.Json;

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
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/department"));
            var pageSize = PaginationConstants.DepartmentPageSize;

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách phòng ban.";
                return View(new List<DepartmnentViewDto>());
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var pagedResult = JsonSerializer.Deserialize<PagedResult<DepartmnentViewDto>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Lấy dữ liệu từ PagedResult
            var department = pagedResult?.Items?.ToList() ?? new List<DepartmnentViewDto>();
            var totalItems = pagedResult?.TotalCount ?? 0;
            var totalPages = pagedResult?.TotalPages ?? 1;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentSearch = search;

            return View(department);
        }
        [HttpGet("chi-tiet")]        
        
        public async Task<IActionResult> ChiTiet(int id)
        {
            var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/department/{id}/detail"));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "Không tìm thấy phòng ban.";
                }
                else
                {
                    TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết khóa học.";
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
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction("Index");
            }

            return View(dept);

        }
        
        // GET: PhongBan/TaoMoi
        public IActionResult TaoMoi()
        {
            return View();
        }
        
        // POST: PhongBan/TaoMoi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TaoMoi(CreateDepartmentDto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // TODO: Call API to create department
                    // await _departmentService.CreateDepartmentAsync(model);
                    
                    TempData["SuccessMessage"] = "Tạo phòng ban thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            
            return View(model);
        }
        
        //public IActionResult ChinhSua(int id)
        //{
        //    // TODO: Replace with actual API call
        //    var department = ;
            
        //    if (department == null)
        //    {
        //        return NotFound();
        //    }
            
        //    return View(department);
        //}
        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ChinhSua(int id, UpdateDepartmentDto model)
        //{
        //    if (id != model.DepartmentId)
        //    {
        //        return NotFound();
        //    }
            
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            // TODO: Call API to update department
        //            // await _departmentService.UpdateDepartmentAsync(model);
                    
        //            TempData["SuccessMessage"] = "Cập nhật phòng ban thành công!";
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
        //        }
        //    }
            
        //    return View(model);
        //}
        
        // POST: PhongBan/Xoa/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Xoa(int id)
        {
            try
            {
                // TODO: Call API to delete department
                // await _departmentService.DeleteDepartmentAsync(id);
                
                TempData["SuccessMessage"] = "Xóa phòng ban thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }
        
    }
}
