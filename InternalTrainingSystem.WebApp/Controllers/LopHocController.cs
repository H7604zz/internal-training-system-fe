using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Models.ViewModels;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = UserRoles.TrainingDepartment + "," + UserRoles.DirectManager)]
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
        [Authorize(Roles = UserRoles.TrainingDepartment +"," +UserRoles.DirectManager + "," + UserRoles.Mentor + "," + UserRoles.Staff)]
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

                // Lấy lịch học của lớp
                var scheduleResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/class/{id}/schedule"));
                List<ScheduleItemResponseDto> schedules = new();
                
                if (scheduleResponse.IsSuccessStatusCode)
                {
                    var scheduleContent = await scheduleResponse.Content.ReadAsStringAsync();
                    
                    // Kiểm tra nội dung không rỗng và là JSON hợp lệ
                    if (!string.IsNullOrWhiteSpace(scheduleContent) && scheduleContent.Trim().StartsWith("["))
                    {
                        schedules = JsonSerializer.Deserialize<List<ScheduleItemResponseDto>>(scheduleContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<ScheduleItemResponseDto>();
                    }
                }

                ViewBag.Schedules = schedules;

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
        [Authorize(Roles = UserRoles.TrainingDepartment)]
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
                    return RedirectToAction("DangNhap", "XacThuc");
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

        [HttpGet("by-course/{courseId}")]
        [Authorize(Roles = UserRoles.TrainingDepartment + "," 
                + UserRoles.DirectManager + "," + UserRoles.Staff + "," + UserRoles.Mentor)]
        public async Task<IActionResult> GetClassesByCourse(int courseId)
        {
            try
            {
                if (courseId <= 0)
                {
                    return BadRequest(new { success = false, message = "ID khóa học không hợp lệ." });
                }

                // Call API to get classes by course
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/class/by-course/{courseId}"));

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var cleanMessage = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể tải danh sách lớp học.";
                    
                    return StatusCode((int)response.StatusCode, new 
                    { 
                        success = false, 
                        message = cleanMessage
                    });
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var classes = JsonSerializer.Deserialize<List<ClassListDto>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ClassListDto>();
                
                return Ok(new
                {
                    success = true,
                    data = classes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Đã xảy ra lỗi khi tải danh sách lớp học." 
                });
            }
        }

        [HttpGet("danh-sach-mentor")]
        public async Task<IActionResult> DanhSachMentor()
        {
            try
            {
                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/user/by-role?role=Mentor"));

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var mentors = JsonSerializer.Deserialize<List<UserDetailResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return Ok(mentors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting mentors list");
                return StatusCode(500, "Đã xảy ra lỗi khi tải danh sách mentor.");
            }
        }

        [HttpPost("them-mentor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMentor([FromBody] AddMentorRequest request)
        {
            try
            {
                if (request == null || request.ClassId <= 0 || string.IsNullOrEmpty(request.MentorId))
                {
                    return BadRequest("Dữ liệu không hợp lệ.");
                }

                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var json = JsonSerializer.Serialize(new { mentorId = request.MentorId });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(
                    Utilities.GetAbsoluteUrl($"api/class/{request.ClassId}/mentor"),
                    content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Thêm mentor thành công.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding mentor to class {ClassId}", request?.ClassId);
                return StatusCode(500, "Đã xảy ra lỗi khi thêm mentor.");
            }
        }

        [HttpPost("luu-lich-hoc")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuLichHoc([FromBody] SaveScheduleRequest request)
        {
            try
            {
                if (request == null || request.ClassId <= 0 || request.Schedules == null || !request.Schedules.Any())
                {
                    return BadRequest("Dữ liệu không hợp lệ.");
                }

                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var json = JsonSerializer.Serialize(request.Schedules);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/class/{request.ClassId}/schedule"),
                    content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Lưu lịch học thành công.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving schedule for class {ClassId}", request?.ClassId);
                return StatusCode(500, "Đã xảy ra lỗi khi lưu lịch học.");
            }
        }

        [HttpPost("thiet-lap")]
        [Authorize(Roles = UserRoles.TrainingDepartment)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThietLap([FromBody] SetupClassRequest request)
        {
            try
            {
                if (request == null || request.ClassId <= 0 || string.IsNullOrEmpty(request.MentorId) ||
                    request.WeeklySchedules == null || !request.WeeklySchedules.Any())
                {
                    return BadRequest("Dữ liệu không hợp lệ.");
                }

                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl("api/class/create-weekly"),
                    content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Thiết lập lớp học thành công.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting up class {ClassId}", request?.ClassId);
                return StatusCode(500, "Đã xảy ra lỗi khi thiết lập lớp học.");
            }
        }

        [HttpPut("doi-lich-hoc/{scheduleId}")]
        [Authorize(Roles = UserRoles.Mentor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiLichHoc(int scheduleId, [FromBody] RescheduleRequest request)
        {
            try
            {
                if (scheduleId <= 0 || request == null)
                {
                    return BadRequest("Dữ liệu không hợp lệ.");
                }

                if (string.IsNullOrWhiteSpace(request.NewLocation))
                {
                    return BadRequest("Phòng học không được để trống.");
                }

                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(
                    Utilities.GetAbsoluteUrl($"api/class/reschedule/{scheduleId}"),
                    content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Đổi lịch học thành công.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var cleanMessage = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể đổi lịch học.";
                    return StatusCode((int)response.StatusCode, cleanMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rescheduling schedule {ScheduleId}", scheduleId);
                return StatusCode(500, "Đã xảy ra lỗi khi đổi lịch học.");
            }
        }

        [HttpGet("cua-toi")]
        [Authorize(Roles = UserRoles.Mentor + "," + UserRoles.Staff)]
        public async Task<IActionResult> CuaToi()
        {
            try
            {
                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Gọi API để lấy danh sách lớp học của user hiện tại
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/class/my"));

                List<MyClassDto> myClasses;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    myClasses = JsonSerializer.Deserialize<List<MyClassDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<MyClassDto>();
                }
                else
                {
                    myClasses = new List<MyClassDto>();
                }

                return View(myClasses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting my classes");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách lớp học.";
                return View(new List<MyClassDto>());
            }
        }

        /// <summary>
        /// Gửi yêu cầu đổi lớp
        /// </summary>
        [HttpPost("swap-class/request")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> RequestSwapClass([FromBody] SwapClassRequestDto request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl("api/class/request-swap"),
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Gửi yêu cầu đổi lớp thành công");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while requesting class swap");
                return StatusCode(500, "Đã xảy ra lỗi khi gửi yêu cầu đổi lớp");
            }
        }

        /// <summary>
        /// Lấy chi tiết lớp học dưới dạng JSON
        /// </summary>
        [HttpGet("json/class-detail/{id}")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> GetClassDetailJson(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/class/{id}"));
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var classDetail = JsonSerializer.Deserialize<ClassDetailDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return Ok(classDetail);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class detail");
                return StatusCode(500, "Đã xảy ra lỗi");
            }
        }

        /// <summary>
        /// Hiển thị danh sách yêu cầu đổi lớp
        /// </summary>
        [HttpGet("yeu-cau-doi-lop")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> YeuCauDoiLop()
        {
            try
            {
                // Check authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/class/my-swap-requests"));

                List<SwapRequestDto> swapRequests;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    swapRequests = JsonSerializer.Deserialize<List<SwapRequestDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<SwapRequestDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    swapRequests = new List<SwapRequestDto>();
                }
                else
                {
                    swapRequests = new List<SwapRequestDto>();
                    TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách yêu cầu.";
                }

                return View(swapRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting swap requests");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách yêu cầu đổi lớp.";
                return View(new List<SwapRequestDto>());
            }
        }

        /// <summary>
        /// Phản hồi yêu cầu đổi lớp
        /// </summary>
        [HttpPost("swap-class/respond")]
        [Authorize(Roles = UserRoles.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondSwapClass([FromBody] RespondSwapRequestDto request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl("api/class/respond-swap-request"),
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Phản hồi yêu cầu thành công");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while responding to swap request");
                return StatusCode(500, "Đã xảy ra lỗi khi phản hồi yêu cầu");
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên trong lớp để nhập điểm
        /// </summary>
        [HttpGet("{classId}/users")]
        [Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> GetUsersInClass(int classId)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized();
                }

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/class/{classId}/users"));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<UsersInClassResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return Ok(result);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users in class {ClassId}", classId);
                return StatusCode(500, "Đã xảy ra lỗi khi tải danh sách nhân viên.");
            }
        }

        /// <summary>
        /// Nhập điểm cuối kỳ cho nhân viên
        /// </summary>
        [HttpPut("scores-final")]
        [Authorize(Roles = UserRoles.Mentor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateScoresFinal([FromBody] ScoreFinalRequest request)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized();
                }

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(Utilities.GetAbsoluteUrl("api/class/scores-final"), content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating scores");
                return StatusCode(500, "Đã xảy ra lỗi khi lưu điểm.");
            }
        }
    }
}