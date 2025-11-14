using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using InternalTrainingSystem.WebApp.Models;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("nguoi-dung")]
    public class NguoiDungController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NguoiDungController(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: nguoi-dung/them-moi
        [HttpGet("them-moi")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ThemMoi()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "Auth");
                }

                // Lấy danh sách phòng ban
                var departments = await GetDepartmentsAsync();
                ViewBag.Departments = departments;

                return View(new CreateStaffDto());
            }
            catch (Exception)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải trang thêm mới người dùng.";
                return RedirectToAction("Index", "TrangChu");
            }
        }

        // POST: nguoi-dung/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ThemMoi(CreateStaffDto model)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "Auth");
                }

                if (!ModelState.IsValid)
                {
                    var departments = await GetDepartmentsAsync();
                    ViewBag.Departments = departments;
                    return View(model);
                }

                // Tạo request DTO khớp với backend CreateUserDto
                var createUserDto = new
                {
                    EmployeeId = model.EmployeeId,
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    DepartmentId = model.DepartmentId,
                    Position = model.Position,
                    RoleName = "Staff" // Role mặc định là Staff
                };

                var json = JsonSerializer.Serialize(createUserDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gọi API POST /api/user
                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl("api/user"), 
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Backend trả về object với Message và Email
                    try
                    {
                        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var message = result.GetProperty("Message").GetString() ?? "Tạo nhân viên thành công!";
                        TempData["Success"] = message;
                    }
                    catch
                    {
                        TempData["Success"] = "Tạo nhân viên thành công! Email kích hoạt đã được gửi.";
                    }
                    
                    // Reset form bằng cách redirect
                    return RedirectToAction("ThemMoi");
                }
                else
                {
                    // Xử lý lỗi từ API - Backend trả về plain text message
                    var errorMessage = responseContent.Trim('"');
                    TempData["Error"] = errorMessage;

                    var departments = await GetDepartmentsAsync();
                    ViewBag.Departments = departments;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                var departments = await GetDepartmentsAsync();
                ViewBag.Departments = departments;
                return View(model);
            }
        }

        // GET: nguoi-dung/thong-tin-ca-nhan
        [HttpGet("thong-tin-ca-nhan")]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "Auth");
                }

                // Lấy thông tin user từ API
                var userProfile = await GetProfileAsync();
                
                if (userProfile != null)
                {
                    return View(userProfile);
                }
                
                // Nếu không lấy được profile, redirect về trang chủ với thông báo lỗi
                TempData["Error"] = "Không thể lấy thông tin cá nhân.";
                return RedirectToAction("Index", "TrangChu");
            }
            catch (Exception)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin cá nhân.";
                return RedirectToAction("Index", "TrangChu");
            }
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost("cap-nhat-thong-tin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatThongTin(UpdateProfileDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var userProfile = await GetProfileAsync();
                    if (userProfile != null)
                    {
                        ViewBag.ShowUpdateForm = true;
                        return View("ThongTinCaNhan", userProfile);
                    }

                    TempData["Error"] = "Không thể lấy thông tin cá nhân để hiển thị.";
                    return RedirectToAction("ThongTinCaNhan");
                }

                // Gọi API update
                var message = await UpdateProfileAsync(model);

                TempData["Success"] = message; 
                return RedirectToAction("ThongTinCaNhan");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message; 
                return RedirectToAction("ThongTinCaNhan");
            }
        }

        /// <summary>
        /// Helper method để lấy danh sách phòng ban
        /// </summary>
        private async Task<List<DepartmnentViewDto>> GetDepartmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/department"));

                if (!response.IsSuccessStatusCode)
                {
                    return new List<DepartmnentViewDto>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var departments = JsonSerializer.Deserialize<List<DepartmnentViewDto>>(
                    responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return departments ?? new List<DepartmnentViewDto>();
            }
            catch
            {
                return new List<DepartmnentViewDto>();
            }
        }

        /// <summary>
        /// Helper method để lấy thông tin user profile
        /// </summary>
        private async Task<UserProfileDto?> GetProfileAsync()
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/profile"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var profileResponse = JsonSerializer.Deserialize<UserProfileDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return profileResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TokenHelpers.ClearTokens(_httpContextAccessor);
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method để update profile
        /// </summary>
        private async Task<string> UpdateProfileAsync(UpdateProfileDto model)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PatchAsync(Utilities.GetAbsoluteUrl("api/user/update-profile"), jsonContent);

            var message = await response.Content.ReadAsStringAsync();
            message = message.Trim('"');

            if (response.IsSuccessStatusCode)
            {
                return message;
            }
            else
            {
                throw new Exception(message);
            }
        }
    }
}
