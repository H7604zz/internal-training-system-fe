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

        // GET: nguoi-dung
        [HttpGet]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Build query string
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                }

                var queryString = string.Join("&", queryParams);
                var apiUrl = Utilities.GetAbsoluteUrl($"api/user?{queryString}");

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PagedResult<UserListDto>>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    ViewBag.CurrentSearch = search;
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;

                    // Lấy danh sách roles
                    var roles = await GetRolesAsync();
                    ViewBag.Roles = roles;

                    return View(result ?? new PagedResult<UserListDto>());
                }
                else
                {
                    TempData["Error"] = "Không thể tải danh sách người dùng.";
                    return View(new PagedResult<UserListDto>());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return View(new PagedResult<UserListDto>());
            }
        }

        /// <summary>
        /// Helper method để lấy danh sách roles
        /// </summary>
        private async Task<List<RoleDto>> GetRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/roles"));

                if (!response.IsSuccessStatusCode)
                {
                    return new List<RoleDto>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<RoleDto>>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return roles ?? new List<RoleDto>();
            }
            catch
            {
                return new List<RoleDto>();
            }
        }

        // POST: nguoi-dung/khoa-tai-khoan
        [HttpPost("khoa-tai-khoan")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> KhoaTaiKhoan([FromBody] ToggleUserStatusRequest request)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl("api/auth/toggle-status"),
                    jsonContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var message = responseContent.Trim('\"');
                    return Json(new { success = true, message = message });
                }
                else
                {
                    var message = responseContent.Trim('\"');
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // POST: nguoi-dung/doi-role
        [HttpPost("doi-role")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> DoiRole([FromBody] ChangeUserRoleRequest request)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl("api/auth/change-role"),
                    jsonContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var message = result.GetProperty("message").GetString();
                    return Json(new { success = true, message = message });
                }
                else
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var message = result.GetProperty("message").GetString();
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
        // GET: nguoi-dung/them-moi
        [HttpGet("them-moi")]
        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.HR)]
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

                // Kiểm tra role của user hiện tại
                var isAdmin = User.IsInRole(UserRoles.Administrator);
                ViewBag.IsAdmin = isAdmin;

                // Nếu là Admin, lấy danh sách roles
                if (isAdmin)
                {
                    var roles = await GetRolesAsync();
                    ViewBag.Roles = roles;
                }

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
        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.HR )]
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
                    
                    // Kiểm tra role để trả về view đúng
                    var isAdmin = User.IsInRole(UserRoles.Administrator);
                    ViewBag.IsAdmin = isAdmin;
                    if (isAdmin)
                    {
                        var roles = await GetRolesAsync();
                        ViewBag.Roles = roles;
                    }
                    
                    return View(model);
                }

                // Xác định RoleName dựa trên role của người dùng hiện tại
                string roleName = UserRoles.Staff;
                if (User.IsInRole(UserRoles.Administrator))
                {
                    roleName = string.IsNullOrWhiteSpace(model.RoleName) ? "Staff" : model.RoleName;
                }
                else if (User.IsInRole(UserRoles.HR))
                {
                    roleName = "Staff";
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
                    RoleName = roleName
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
                    
                    // Kiểm tra role để trả về view đúng
                    var isAdmin = User.IsInRole(UserRoles.Administrator);
                    ViewBag.IsAdmin = isAdmin;
                    if (isAdmin)
                    {
                        var roles = await GetRolesAsync();
                        ViewBag.Roles = roles;
                    }
                    
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                var departments = await GetDepartmentsAsync();
                ViewBag.Departments = departments;
                
                // Kiểm tra role để trả về view đúng
                var isAdmin = User.IsInRole(UserRoles.Administrator);
                ViewBag.IsAdmin = isAdmin;
                if (isAdmin)
                {
                    var roles = await GetRolesAsync();
                    ViewBag.Roles = roles;
                }
                
                return View(model);
            }
        }

        // GET: nguoi-dung/thong-tin-ca-nhan
        [HttpGet("thong-tin-ca-nhan")]
        [Authorize]
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
        [Authorize]
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

        /// <summary>
        /// Xem thời khóa biểu
        /// </summary>
        [HttpGet("thoi-khoa-bieu")]
        [Authorize(Roles = UserRoles.Staff + "," + UserRoles.Mentor)]
        public async Task<IActionResult> ThoiKhoaBieu()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Lấy danh sách lịch học từ API
                var schedules = await GetSchedulesAsync();
                
                if (schedules == null)
                {
                    TempData["Error"] = "Không thể lấy thời khóa biểu.";
                    return View(new List<ScheduleItemResponseDto>());
                }

                return View(schedules);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return View(new List<ScheduleItemResponseDto>());
            }
        }

        /// <summary>
        /// Helper method để lấy danh sách lịch học
        /// </summary>
        private async Task<List<ScheduleItemResponseDto>?> GetSchedulesAsync()
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/schedule"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var schedules = JsonSerializer.Deserialize<List<ScheduleItemResponseDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return schedules ?? new List<ScheduleItemResponseDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TokenHelpers.ClearTokens(_httpContextAccessor);
                    return null;
                }
                else
                {
                    return new List<ScheduleItemResponseDto>();
                }
            }
            catch (Exception)
            {
                return new List<ScheduleItemResponseDto>();
            }
        }

        /// <summary>
        /// Trang điểm danh cho một buổi học
        /// </summary>
        [HttpGet("diem-danh/{scheduleId}")]
        [Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> DiemDanh(int scheduleId)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Lấy danh sách attendance từ API
                var attendances = await GetAttendanceByScheduleAsync(scheduleId);
                
                if (attendances == null)
                {
                    TempData["Error"] = "Không thể lấy danh sách điểm danh.";
                    return RedirectToAction("ThoiKhoaBieu");
                }

                ViewBag.ScheduleId = scheduleId;
                return View(attendances);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("ThoiKhoaBieu");
            }
        }

        /// <summary>
        /// Lưu điểm danh
        /// </summary>
        [HttpPost("diem-danh/{scheduleId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> LuuDiemDanh(int scheduleId, List<AttendanceRequest> attendanceList)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                if (attendanceList == null || !attendanceList.Any())
                {
                    TempData["Error"] = "Danh sách điểm danh trống.";
                    return RedirectToAction("DiemDanh", new { scheduleId });
                }

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(attendanceList),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/Class/{scheduleId}/attendance"),
                    jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Lưu điểm danh thành công!";
                    return RedirectToAction("ThoiKhoaBieu");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = errorMessage.Trim('"');
                    return RedirectToAction("DiemDanh", new { scheduleId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("DiemDanh", new { scheduleId });
            }
        }

        /// <summary>
        /// Helper method để lấy danh sách attendance theo scheduleId
        /// </summary>
        private async Task<List<AttendanceResponse>?> GetAttendanceByScheduleAsync(int scheduleId)
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/Class/schedules/{scheduleId}/attendance"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var attendances = JsonSerializer.Deserialize<List<AttendanceResponse>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return attendances ?? new List<AttendanceResponse>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TokenHelpers.ClearTokens(_httpContextAccessor);
                    return null;
                }
                else
                {
                    return new List<AttendanceResponse>();
                }
            }
            catch (Exception)
            {
                return new List<AttendanceResponse>();
            }
        }

        /// <summary>
        /// Xem báo cáo tình hình học tập
        /// </summary>
        [HttpGet("bao-cao-tinh-hinh")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> BaoCaoTinhHinh()
        {
            try
            {
                // Kiểm tra đăng nhập
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Lấy báo cáo từ API
                var courseSummaries = await GetCourseSummaryAsync();
                
                if (courseSummaries == null)
                {
                    TempData["Error"] = "Không thể lấy báo cáo tình hình học tập.";
                    return View(new List<UserCourseSummaryDto>());
                }

                return View(courseSummaries);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return View(new List<UserCourseSummaryDto>());
            }
        }

        /// <summary>
        /// Helper method để lấy báo cáo tình hình học tập
        /// </summary>
        private async Task<List<UserCourseSummaryDto>?> GetCourseSummaryAsync()
        {
            try
            {
                var token = TokenHelpers.GetAccessToken(_httpContextAccessor);
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/course-summary"));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var summaries = JsonSerializer.Deserialize<List<UserCourseSummaryDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return summaries ?? new List<UserCourseSummaryDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TokenHelpers.ClearTokens(_httpContextAccessor);
                    return null;
                }
                else
                {
                    return new List<UserCourseSummaryDto>();
                }
            }
            catch (Exception)
            {
                return new List<UserCourseSummaryDto>();
            }
        }
    }
}
