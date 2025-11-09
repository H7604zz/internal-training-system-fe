using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Extensions;
using InternalTrainingSystem.WebApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using InternalTrainingSystem.WebApp.Models;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("khoa-hoc")]
    public class KhoaHocController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KhoaHocController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KhoaHocController(
            IHttpClientFactory httpClientFactory,
            ILogger<KhoaHocController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string status, int page = 1)
        {
            try
            {
                // Sử dụng page size cố định từ constants cho trang chính khóa học
                var pageSize = PaginationConstants.CoursePageSize;
                
                // Gọi API với các tham số filter và pagination - backend sẽ xử lý
                var queryParams = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(status))
                    queryParams += $"&status={Uri.EscapeDataString(status)}";

                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course{queryParams}"));
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get courses. Status: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách khóa học.";
                    return View(new List<CourseDto>());
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var pagedResult = JsonSerializer.Deserialize<PagedResult<CourseDto>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Lấy dữ liệu từ PagedResult
                var courses = pagedResult?.Items?.ToList() ?? new List<CourseDto>();
                var totalItems = pagedResult?.TotalCount ?? 0;
                var totalPages = pagedResult?.TotalPages ?? 1;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems; // Tổng số bản ghi từ DB (đã được filter)
                ViewBag.Status = status;

                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading course list");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách khóa học.";
                return View(new List<CourseDto>());
            }
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}/detail"));
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Không tìm thấy khóa học.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết khóa học.";
                    }
                    return RedirectToAction("Index");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<CourseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading course details for id {CourseId}", id);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết khóa học.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> ChinhSua(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}"));
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Không tìm thấy khóa học.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học.";
                    }
                    return RedirectToAction("Index");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<CourseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                await ReloadFormData();

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while loading course {id} for editing");
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChinhSua(CourseDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await ReloadFormData();
                    return View(model);
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(Utilities.GetAbsoluteUrl($"api/course/{model.CourseId}"), content);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
                    return RedirectToAction("ChiTiet", new { id = model.CourseId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể cập nhật khóa học. Vui lòng thử lại.";
                    await ReloadFormData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating course {model.CourseId}");
                TempData["Error"] = "Đã xảy ra lỗi khi cập nhật khóa học.";
                await ReloadFormData();
                return View(model);
            }
        }

        [HttpGet("tao-moi")]
        public async Task<IActionResult> TaoMoi()
        {
            await ReloadFormData();
            return View(new CreateFullCourseDto());
        }

        [HttpPost("tao-moi")]
        public async Task<IActionResult> TaoMoi(CreateFullCourseDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage);
                    foreach (var error in errors)
                    {
                        TempData["Error"] = error;
                    }

                    // Reload dropdown data nhưng giữ lại toàn bộ thông tin form bao gồm selected departments
                    await ReloadFormData(model);
                    return View(model);
                }

                // Prepare metadata DTO khớp với backend CreateCourseMetadataDto
                var metadata = new
                {
                    CourseCode = model.CourseCode,
                    CourseName = model.CourseName,
                    Description = model.Description,
                    CourseCategoryId = model.CourseCategoryId,
                    Duration = model.Duration,
                    Level = model.Level,
                    IsOnline = model.IsOnline,
                    IsMandatory = model.IsMandatory,
                    Departments = model.SelectedDepartmentIds, // Backend expect "Departments", not "SelectedDepartmentIds"
                    Modules = model.Modules.Select(m => new
                    {
                        Title = m.Title,
                        Description = m.Description,
                        OrderIndex = m.OrderIndex,
                        Lessons = m.Lessons.Select(l => new
                        {
                            Title = l.Title,
                            OrderIndex = l.OrderIndex,
                            Type = (int)l.Type, // Enum to int: Video=1, Reading=2, Quiz=3
                            Description = l.Description,
                            ContentUrl = l.ContentUrl, // Video URL
                            MainFileIndex = l.MainFileIndex, // Reading file hoặc Quiz file index
                            AttachmentUrl = l.AttachmentUrl, // Document URL hoặc Additional Document URL
                            QuizTitle = l.QuizTitle,
                            IsQuizExcel = l.IsQuizExcel,
                            QuizTimeLimit = l.QuizTimeLimit,
                            QuizMaxAttempts = l.QuizMaxAttempts,
                            QuizPassingScore = l.QuizPassingScore
                        }).ToList()
                    }).ToList()
                };

                // Serialize metadata to JSON string
                var metadataJson = JsonSerializer.Serialize(metadata);

                // Create multipart form data
                using var formContent = new MultipartFormDataContent();
                
                // Add metadata as string content
                formContent.Add(new StringContent(metadataJson, Encoding.UTF8, "application/json"), "metadata");

                // Add lesson files (LessonDocumentFiles and LessonQuizFiles)
                if (model.LessonFiles != null && model.LessonFiles.Any())
                {
                    foreach (var file in model.LessonFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var fileContent = new StreamContent(file.OpenReadStream());
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                            formContent.Add(fileContent, "lessonFiles", file.FileName);
                        }
                    }
                }

                // Send request to API
                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/course"), formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Thêm khóa học mới thành công! Khóa học đã được gửi để chờ phê duyệt.";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorMessage = !string.IsNullOrEmpty(responseContent) ? responseContent.Trim('"') : "Không thể tạo khóa học. Vui lòng thử lại.";
                    
                    // Try parse JSON error response
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (errorObj.TryGetProperty("message", out var msgElement))
                        {
                            errorMessage = msgElement.GetString() ?? errorMessage;
                        }
                        // Check for validation errors array
                        if (errorObj.TryGetProperty("errors", out var errorsElement))
                        {
                            var errors = errorsElement.EnumerateArray().Select(e => e.GetString()).ToList();
                            errorMessage += " " + string.Join(", ", errors);
                        }
                    }
                    catch
                    {
                        // Use raw response if JSON parse fails
                    }
                    
                    TempData["Error"] = errorMessage;
                    // Reload dropdown data nhưng giữ lại toàn bộ thông tin form bao gồm selected departments
                    await ReloadFormData(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new course. Model state: {ModelState}, Module count: {ModuleCount}, Department count: {DepartmentCount}", 
                    string.Join("; ", ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")),
                    model.Modules?.Count ?? 0,
                    model.SelectedDepartmentIds?.Count ?? 0);
                
                TempData["Error"] = "Đã xảy ra lỗi khi thêm khóa học.";
                // Reload dropdown data nhưng giữ lại toàn bộ thông tin form bao gồm selected departments
                await ReloadFormData(model);
                return View(model);
            }
        }

        /// <summary>
        /// Helper method để reload dropdown data cho form
        /// </summary>
        private async Task ReloadFormData(CreateFullCourseDto model = null)
        {
            try
            {
                // Load categories
                var categoriesResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("/categories"));
                if (categoriesResponse.IsSuccessStatusCode)
                {
                    var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    ViewBag.Categories = categories ?? new List<CategoryDto>();
                }
                else
                {
                    ViewBag.Categories = new List<CategoryDto>();
                }

                // Load departments
                var departmentsResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/department"));
                if (departmentsResponse.IsSuccessStatusCode)
                {
                    var departmentsContent = await departmentsResponse.Content.ReadAsStringAsync();
                    var departments = JsonSerializer.Deserialize<List<DepartmnentViewDto>>(departmentsContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    ViewBag.Departments = departments ?? new List<DepartmnentViewDto>();
                }
                else
                {
                    ViewBag.Departments = new List<DepartmnentViewDto>();
                }
                
                // Preserve selected departments nếu có
                if (model?.SelectedDepartmentIds != null && model.SelectedDepartmentIds.Any())
                {
                    ViewBag.SelectedDepartmentIds = model.SelectedDepartmentIds;
                    _logger.LogInformation("Preserved {Count} selected departments: [{DepartmentIds}]", 
                        model.SelectedDepartmentIds.Count, 
                        string.Join(", ", model.SelectedDepartmentIds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred while reloading form data");
                // Fallback to empty lists if service calls fail
                ViewBag.Categories = new List<dynamic>();
                ViewBag.Departments = new List<dynamic>();
                
                // Still preserve selected departments even if API calls fail
                if (model?.SelectedDepartmentIds != null && model.SelectedDepartmentIds.Any())
                {
                    ViewBag.SelectedDepartmentIds = model.SelectedDepartmentIds;
                }
            }
        }

        

        [HttpGet("danh-sach-nhan-vien/{id}")]
        public async Task<IActionResult> DanhSachNhanVien(int id, string search, string status, int page = 1)
        {
            try
            {
                // Get course details
                var courseResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}/detail"));
                if (!courseResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                var courseContent = await courseResponse.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<CourseDto>(courseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                // Sử dụng page size cố định từ constants
                var pageSize = PaginationConstants.StaffPageSize;

                // Build query parameters for eligible staff
                var queryParams = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(search))
                    queryParams += $"&search={Uri.EscapeDataString(search)}";
                if (!string.IsNullOrEmpty(status))
                    queryParams += $"&status={Uri.EscapeDataString(status)}";

                // lấy danh sách nhân viên đủ điều kiện
                var staffResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}/eligible-staff{queryParams}"));
                
                PagedResult<EligibleStaffDto> eligibleStaffResult;
                List<EligibleStaffDto> staff;

                if (staffResponse.IsSuccessStatusCode)
                {
                    var staffContent = await staffResponse.Content.ReadAsStringAsync();
                    eligibleStaffResult = JsonSerializer.Deserialize<PagedResult<EligibleStaffDto>>(staffContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new PagedResult<EligibleStaffDto>();
                    
                    staff = eligibleStaffResult.Items?.ToList() ?? new List<EligibleStaffDto>();
                }
                else
                {
                    _logger.LogWarning("Failed to get eligible staff for course {CourseId}. Status: {StatusCode}", id, staffResponse.StatusCode);
                    eligibleStaffResult = new PagedResult<EligibleStaffDto>();
                    staff = new List<EligibleStaffDto>();
                }

                ViewBag.Course = course;
                ViewBag.CourseId = id;
                ViewBag.Search = search;
                ViewBag.Status = status;
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = eligibleStaffResult.TotalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = eligibleStaffResult.TotalCount; // Tổng số bản ghi từ API
                ViewBag.HasPreviousPage = eligibleStaffResult.HasPreviousPage;
                ViewBag.HasNextPage = eligibleStaffResult.HasNextPage;

                // Get current user role for button visibility
                ViewBag.UserRole = await UserProfileHelper.GetCurrentUserRoleAsync(_httpClient, _httpContextAccessor);
                
                // Check if enrollments are finalized
                bool isFinalized = false;
                try
                {
                    // Try to check if there's a finalization status API
                    var finalizationResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}/finalization-status"));
                    if (finalizationResponse.IsSuccessStatusCode)
                    {
                        var finalizationContent = await finalizationResponse.Content.ReadAsStringAsync();
                        isFinalized = JsonSerializer.Deserialize<bool>(finalizationContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else
                    {
                        // Fallback: check if there are any enrolled students (indicates finalization has occurred)
                        isFinalized = staff.Any(s => s.Status == "Enrolled" || s.Status == "InProgress" || s.Status == "Completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check finalization status for course {CourseId}", id);
                    // Fallback: check if there are any enrolled students
                    isFinalized = staff.Any(s => s.Status == "Enrolled" || s.Status == "InProgress" || s.Status == "Completed");
                }
                ViewBag.IsFinalized = isFinalized;
                
                // Get total count of enrolled employees using the confirmed-staff/count API
                try
                {
                    var confirmedCountResponse = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}/confirmed-staff/count"));
                    
                    if (confirmedCountResponse.IsSuccessStatusCode)
                    {
                        var confirmedCountContent = await confirmedCountResponse.Content.ReadAsStringAsync();
                        ViewBag.TotalEnrolledCount = JsonSerializer.Deserialize<int>(confirmedCountContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else
                    {
                        ViewBag.TotalEnrolledCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get confirmed staff count for course {CourseId}", id);
                    ViewBag.TotalEnrolledCount = 0;
                }
                
                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting employee list for course {id}");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách nhân viên.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("gui-thong-bao-nhan-vien/{courseId}")]
        public async Task GuiThongBaoNhanVien(int courseId)
        {
            try
            {
                if (courseId <= 0)
                {
                    return;
                }

                // Gọi API để gửi mail thông báo cho nhân viên đủ điều kiện
                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl($"api/notification/{courseId}/notify-eligible-users"), null);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Đã gửi thông báo thành công cho tất cả nhân viên đủ điều kiện!";
                }
                //else
                //{
                //    var errorContent = await response.Content.ReadAsStringAsync();
                //    var errorMessage = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể gửi thông báo. Vui lòng thử lại.";
                //    TempData["Error"] = errorMessage;
                //}
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi gửi thông báo. Vui lòng thử lại!";
            }
        }


        [HttpGet("phe-duyet/{id}")]
        public async Task<IActionResult> PheDuyet(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{id}/detail"));
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Không tìm thấy khóa học cần phê duyệt.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học để phê duyệt.";
                    }
                    return RedirectToAction("Index");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<CourseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (course == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học cần phê duyệt.";
                    return RedirectToAction("Index");
                }

                if (course.Status != CourseStatus.Pending)
                {
                    TempData["Warning"] = "Khóa học này đã được xử lý phê duyệt.";
                    return RedirectToAction("ChiTiet", new { id });
                }

                ViewBag.ApprovalHistory = await GetApprovalHistory(id);

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while loading course {id} for approval");
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học để phê duyệt.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] CourseApprovalRequest request)
        {
            try
            {
                if (request.CourseId <= 0)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Thông tin yêu cầu không hợp lệ.",
                        ErrorCode = ErrorCode.InvalidRequest
                    });
                }

                var payload = new { NewStatus = CourseStatus.Approved };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), Utilities.GetAbsoluteUrl($"api/course/update-pending-status/{request.CourseId}"))
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseContent);
                        var root = doc.RootElement;
                        var message = root.GetProperty("message").GetString() ?? "Đã phê duyệt khóa học.";
                        return Json(new CourseApprovalResponse
                        {
                            Success = true,
                            Message = message
                        });
                    }
                    catch
                    {
                        return Json(new CourseApprovalResponse
                        {
                            Success = true,
                            Message = responseContent.Trim('"')
                        });
                    }
                }
                else
                {
                    var errorMessage = !string.IsNullOrEmpty(responseContent) ? responseContent.Trim('"') : "Đã xảy ra lỗi khi phê duyệt khóa học.";
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        ErrorCode = ErrorCode.InternalError
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while approving course {request.CourseId}");
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi phê duyệt khóa học. Vui lòng thử lại.",
                    ErrorCode = ErrorCode.InternalError
                });
            }
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] CourseApprovalRequest request)
        {
            try
            {
                if (request.CourseId <= 0 || string.IsNullOrEmpty(request.Reason))
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Vui lòng nhập đầy đủ thông tin và lý do từ chối.",
                        ErrorCode = ErrorCode.InvalidRequest
                    });
                }

                if (request.Reason.Length < 20)
                {
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = "Lý do từ chối phải có ít nhất 20 ký tự.",
                        ErrorCode = ErrorCode.ReasonTooShort
                    });
                }

                var payload = new { NewStatus = CourseStatus.Rejected, RejectReason = request.Reason };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), Utilities.GetAbsoluteUrl($"api/course/update-pending-status/{request.CourseId}"))
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseContent);
                        var root = doc.RootElement;
                        var message = root.GetProperty("message").GetString() ?? "Khóa học đã bị từ chối.";
                        return Json(new CourseApprovalResponse
                        {
                            Success = true,
                            Message = message
                        });
                    }
                    catch
                    {
                        return Json(new CourseApprovalResponse
                        {
                            Success = true,
                            Message = responseContent.Trim('"')
                        });
                    }
                }
                else
                {
                    var errorMessage = !string.IsNullOrEmpty(responseContent) ? responseContent.Trim('"') : "Đã xảy ra lỗi khi từ chối khóa học.";
                    return Json(new CourseApprovalResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        ErrorCode = ErrorCode.InternalError
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while rejecting course {request.CourseId}");
                return Json(new CourseApprovalResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi từ chối khóa học. Vui lòng thử lại.",
                    ErrorCode = ErrorCode.InternalError
                });
            }
        }



        private async Task<List<ApprovalHistoryDto>> GetApprovalHistory(int courseId)
        {
            try
            {
                // Get course details first
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{courseId}"));
                if (!response.IsSuccessStatusCode)
                {
                    return new List<ApprovalHistoryDto>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<CourseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (course == null) return new List<ApprovalHistoryDto>();

                var history = new List<ApprovalHistoryDto>
                {
                    new ApprovalHistoryDto
                    {
                        Id = 1,
                        CourseId = courseId,
                        Action = ApprovalAction.Created,
                        Description = "Khóa học được tạo",
                        ActionBy = course.CreatedBy ?? "Nhân viên đào tạo",
                        ActionDate = course.CreatedDate
                    },
                    new ApprovalHistoryDto
                    {
                        Id = 2,
                        CourseId = courseId,
                        Action = ApprovalAction.Submitted,
                        Description = "Gửi yêu cầu phê duyệt",
                        Note = "Khóa học đã sẵn sàng để phê duyệt",
                        ActionBy = course.CreatedBy ?? "Nhân viên đào tạo",
                        ActionDate = course.CreatedDate.AddMinutes(30)
                    }
                };

                // Add approval/rejection history if exists
                if (course.Status == CourseStatus.Approved && course.ApprovalDate.HasValue)
                {
                    history.Add(new ApprovalHistoryDto
                    {
                        Id = 3,
                        CourseId = courseId,
                        Action = ApprovalAction.Approved,
                        Description = "Khóa học đã được phê duyệt",
                        Note = "Khóa học đã được phê duyệt và có thể tạo lớp học",
                        ActionBy = course.ApprovedBy ?? "Giám đốc",
                        ActionDate = course.ApprovalDate.Value
                    });
                }
                else if (course.Status == CourseStatus.Rejected && course.ApprovalDate.HasValue)
                {
                    history.Add(new ApprovalHistoryDto
                    {
                        Id = 3,
                        CourseId = courseId,
                        Action = ApprovalAction.Rejected,
                        Description = "Khóa học bị từ chối",
                        Note = course.RejectionReason,
                        ActionBy = course.ApprovedBy ?? "Giám đốc",
                        ActionDate = course.ApprovalDate.Value
                    });
                }

                return history.OrderBy(h => h.ActionDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting approval history for course {courseId}");
                return new List<ApprovalHistoryDto>();
            }
        }

        

        /// <summary>
        /// Danh sách khóa học của nhân viên
        /// </summary>
        [HttpGet("danh-sach-khoa-hoc-cua-toi")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> DanhSachKhoaHocCuaToi(string status = "all", int page = 1)
        {
            try
            {
                var id = HttpContext.Session.GetString("Id");
                // Sử dụng page size cố định từ constants cho khóa học của nhân viên
                var pageSize = PaginationConstants.EmployeeCoursePageSize;

                // Gọi API với tham số phân trang và filter
                var queryParams = $"?userId={id}&page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(status) && status != "all")
                    queryParams += $"&status={Uri.EscapeDataString(status)}";

                // Lấy danh sách khóa học từ API - API sẽ tự lấy userId từ JWT token
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/user/courses{queryParams}"));

                PagedResult<EmployeeCourseDto> pagedResult;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    pagedResult = JsonSerializer.Deserialize<PagedResult<EmployeeCourseDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new PagedResult<EmployeeCourseDto>();
                }
                else
                {
                    pagedResult = new PagedResult<EmployeeCourseDto>();
                }

                // Lấy danh sách courses từ pagedResult
                var courses = pagedResult.Items?.ToList() ?? new List<EmployeeCourseDto>();

                // Thống kê số lượng theo trạng thái
                var totalCourses = pagedResult.TotalCount;
               
                // ViewBag cho phân trang và filter
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalCourses;
                ViewBag.CurrentStatus = status;

                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting employee courses");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách khóa học.";
                return RedirectToAction("Index", "TrangChu");
            }
        }

        /// <summary>
        /// Phản hồi tham gia khóa học
        /// </summary>
        [HttpPost("phan-hoi-khoa-hoc")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> PhanHoiKhoaHoc(int courseId, string responseType, string? note = null)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var requestBody = new
                {
                    //EmployeeId = employeeId,
                    CourseId = courseId,
                    ResponseType = responseType,
                    Note = note
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl($"api/course/{courseId}/respond"), content);

                if (response.IsSuccessStatusCode)
                {
                    var message = responseType.ToLower() switch
                    {
                        "accepted" => "Đã xác nhận tham gia khóa học thành công!",
                        "declined" => "Đã từ chối tham gia khóa học.",
                        _ => "Đã cập nhật phản hồi thành công!"
                    };

                    return Json(new { success = true, message = message });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể cập nhật phản hồi. Vui lòng thử lại.";
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phản hồi" });
            }
        }

        [HttpPost("finalize-enrollments")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeEnrollments(int courseId)
        {
            try
            {
                if (courseId <= 0)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ";
                    return RedirectToAction("DanhSachNhanVien", new { id = courseId });
                }

                // Kiểm tra authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn";
                    return RedirectToAction("DangNhap", "Auth");
                }

                // Call API to finalize enrollments
                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/course/{courseId}/finalize-enrollments"), 
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    TempData["Success"] = result.Trim('"'); // Remove quotes from string response
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = errorMessage.Trim('"');
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi chốt danh sách. Vui lòng thử lại sau!";
            }

            return RedirectToAction("DanhSachNhanVien", new { id = courseId });
        }

        /// <summary>
        /// Trang học tập của nhân viên
        /// </summary>
        [HttpGet("hoc-tap/{courseId}")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> HocTap(int courseId)
        {
            try
            {
                // Kiểm tra authentication
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn";
                    return RedirectToAction("DangNhap", "Auth");
                }

                // Get course learning details with progress
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/course/{courseId}/learning"));
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Không tìm thấy khóa học hoặc bạn chưa được ghi danh vào khóa học này.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        TempData["Error"] = "Bạn không có quyền truy cập khóa học này.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin khóa học.";
                    }
                    return RedirectToAction("DanhSachKhoaHocCuaToi");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var courseLearning = JsonSerializer.Deserialize<CourseLearningDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (courseLearning == null)
                {
                    TempData["Error"] = "Không thể tải thông tin khóa học.";
                    return RedirectToAction("DanhSachKhoaHocCuaToi");
                }

                return View(courseLearning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading learning page for course {CourseId}", courseId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải trang học tập.";
                return RedirectToAction("DanhSachKhoaHocCuaToi");
            }
        }

        /// <summary>
        /// Đánh dấu bài học hoàn thành
        /// </summary>
        [HttpPost("hoc-tap/hoan-thanh-bai-hoc")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> HoanThanhBaiHoc([FromBody] CompleteLessonRequest request)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/lesson/{request.LessonId}/complete"), 
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var progressUpdate = JsonSerializer.Deserialize<LessonProgressUpdateDto>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return Json(new { success = true, message = "Đã đánh dấu hoàn thành bài học", progress = progressUpdate });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = errorMessage.Trim('"') });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while completing lesson {LessonId}", request.LessonId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi đánh dấu hoàn thành bài học" });
            }
        }

        /// <summary>
        /// Hiển thị chứng chỉ hoàn thành khóa học
        /// </summary>
        [HttpGet("chung-chi/{courseId}")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> ChungChi(int courseId)
        {
            try
            {
                // Get certificate details from API
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/certificate/course/{courseId}"));
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "Bạn chưa hoàn thành khóa học này để nhận chứng chỉ.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        TempData["Error"] = "Bạn không có quyền xem chứng chỉ này.";
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi tải chứng chỉ.";
                    }
                    return RedirectToAction("DanhSachKhoaHocCuaToi");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var certificate = JsonSerializer.Deserialize<CertificateDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (certificate == null)
                {
                    TempData["Error"] = "Không thể tải thông tin chứng chỉ.";
                    return RedirectToAction("DanhSachKhoaHocCuaToi");
                }

                return View(certificate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading certificate for course {CourseId}", courseId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chứng chỉ.";
                return RedirectToAction("DanhSachKhoaHocCuaToi");
            }
        }

        /// <summary>
        /// Tải xuống chứng chỉ dạng PDF
        /// </summary>
        [HttpGet("chung-chi/{courseId}/tai-xuong")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> TaiXuongChungChi(int courseId)
        {
            try
            {
                // Call API to download certificate PDF
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/certificate/course/{courseId}/download"));
                
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không thể tải xuống chứng chỉ. Vui lòng thử lại sau.";
                    return RedirectToAction("ChungChi", new { courseId });
                }

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"ChungChi_{courseId}_{DateTime.Now:yyyyMMdd}.pdf";
                
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading certificate for course {CourseId}", courseId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải xuống chứng chỉ.";
                return RedirectToAction("ChungChi", new { courseId });
            }
        }
    }


}