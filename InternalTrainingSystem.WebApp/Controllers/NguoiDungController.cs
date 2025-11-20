using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Helpers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Constants;

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
                    return RedirectToAction("DangNhap", "XacThuc");
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
                    return RedirectToAction("DangNhap", "XacThuc");
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
                    var message = responseContent.Trim('"');
                    TempData["Success"] = message;
                    
                    return RedirectToAction("ThemMoi");
                }
                else
                {
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
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
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
        [Authorize(Roles = UserRoles.Staff)]
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
        /// Lấy danh sách chứng chỉ của người dùng
        /// </summary>
        [HttpGet("chung-chi-cua-toi")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> ChungChiCuaToi()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Lấy danh sách chứng chỉ từ API
                var certificates = await GetCertificatesAsync();
                
                if (certificates == null)
                {
                    TempData["Error"] = "Không thể lấy danh sách chứng chỉ.";
                    return View(new List<CertificateDto>());
                }

                return View(certificates);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return View(new List<CertificateDto>());
            }
        }

        /// <summary>
        /// Helper method để lấy danh sách chứng chỉ
        /// </summary>
        private async Task<List<CertificateDto>?> GetCertificatesAsync()
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/certificates"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var certificates = JsonSerializer.Deserialize<List<CertificateDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return certificates ?? new List<CertificateDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TokenHelpers.ClearTokens(_httpContextAccessor);
                    return null;
                }
                else
                {
                    return new List<CertificateDto>();
                }
            }
            catch (Exception)
            {
                return new List<CertificateDto>();
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
