using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace InternalTrainingSystem.WebApp.Controllers
{
    [Route("bai-tap")]
    [Authorize]
    public class BaiTapController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BaiTapController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaiTapController(
            IHttpClientFactory httpClientFactory,
            ILogger<BaiTapController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Danh sách bài tập của lớp học
        /// Mentor: xem tất cả
        /// Staff: chỉ xem nếu thuộc lớp
        /// </summary>
        [HttpGet("{classId}")]
        [Authorize(Roles = UserRoles.Mentor + "," + UserRoles.Staff)]
        public async Task<IActionResult> Index(int classId)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Lấy thông tin lớp học trước
                var classResponse = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/class/{classId}"));

                if (!classResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy lớp học.";
                    return RedirectToAction("CuaToi", "LopHoc");
                }

                var classContent = await classResponse.Content.ReadAsStringAsync();
                var classDetail = JsonSerializer.Deserialize<ClassDetailDto>(classContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Lấy danh sách bài tập
                var response = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{classId}"));

                List<AssignmentDto> assignments;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    assignments = JsonSerializer.Deserialize<List<AssignmentDto>>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                        ?? new List<AssignmentDto>();
                }
                else
                {
                    assignments = new List<AssignmentDto>();
                }

                ViewBag.ClassId = classId;
                ViewBag.ClassName = classDetail?.ClassName ?? "";
                ViewBag.IsMentor = User.IsInRole(UserRoles.Mentor);

                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting assignments for class {ClassId}", classId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách bài tập.";
                return RedirectToAction("CuaToi", "LopHoc");
            }
        }

        /// <summary>
        /// Chi tiết bài tập
        /// </summary>
        [HttpGet("{classId}/chi-tiet/{assignmentId}")]
        public async Task<IActionResult> ChiTiet(int classId, int assignmentId)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                var response = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{classId}/{assignmentId}"));

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy bài tập.";
                    return RedirectToAction("Index", new { classId });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var assignment = JsonSerializer.Deserialize<AssignmentDto>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (assignment == null)
                {
                    TempData["Error"] = "Không tìm thấy bài tập.";
                    return RedirectToAction("Index", new { classId });
                }

                ViewBag.IsMentor = User.IsInRole(UserRoles.Mentor);
                ViewBag.ClassId = classId;

                return View(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting assignment detail {AssignmentId}", assignmentId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết bài tập.";
                return RedirectToAction("Index", new { classId });
            }
        }

        /// <summary>
        /// Trang tạo bài tập mới (Mentor)
        /// </summary>
        [HttpGet("{classId}/tao-moi")]
        [Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> TaoMoi(int classId)
        {
            try
            {
                // Lấy thông tin lớp học
                var classResponse = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/class/{classId}"));

                if (!classResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy lớp học.";
                    return RedirectToAction("CuaToi", "LopHoc");
                }

                var classContent = await classResponse.Content.ReadAsStringAsync();
                var classDetail = JsonSerializer.Deserialize<ClassDetailDto>(classContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ViewBag.ClassId = classId;
                ViewBag.ClassName = classDetail?.ClassName ?? "";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create assignment page");
                TempData["Error"] = "Đã xảy ra lỗi.";
                return RedirectToAction("Index", new { classId });
            }
        }

        /// <summary>
        /// Tạo bài tập mới (Mentor)
        /// </summary>
        [HttpPost("{classId}/tao-moi")]
        [Authorize(Roles = UserRoles.Mentor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoi(int classId, CreateAssignmentForm form)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ.";
                    return RedirectToAction("TaoMoi", new { classId });
                }

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(classId.ToString()), "ClassId");
                formData.Add(new StringContent(form.Title), "Title");
                
                if (!string.IsNullOrEmpty(form.Description))
                    formData.Add(new StringContent(form.Description), "Description");
                
                formData.Add(new StringContent(form.DueAt.ToString("o")), "DueAt");

                if (form.AttachmentFile != null)
                {
                    var fileContent = new StreamContent(form.AttachmentFile.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        form.AttachmentFile.ContentType);
                    formData.Add(fileContent, "AttachmentFile", form.AttachmentFile.FileName);
                }

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{classId}"),
                    formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Tạo bài tập thành công!";
                    return RedirectToAction("Index", new { classId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Không thể tạo bài tập: {errorContent}";
                    return RedirectToAction("TaoMoi", new { classId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating assignment");
                TempData["Error"] = "Đã xảy ra lỗi khi tạo bài tập.";
                return RedirectToAction("TaoMoi", new { classId });
            }
        }

        /// <summary>
        /// Trang sửa bài tập (Mentor)
        /// </summary>
        [HttpGet("{classId}/sua/{assignmentId}")]
        [Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> Sua(int classId, int assignmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{classId}/{assignmentId}"));

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy bài tập.";
                    return RedirectToAction("Index", new { classId });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var assignment = JsonSerializer.Deserialize<AssignmentDto>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (assignment == null)
                {
                    TempData["Error"] = "Không tìm thấy bài tập.";
                    return RedirectToAction("Index", new { classId });
                }

                ViewBag.ClassId = classId;
                ViewBag.AssignmentId = assignmentId;

                return View(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading edit assignment page");
                TempData["Error"] = "Đã xảy ra lỗi.";
                return RedirectToAction("Index", new { classId });
            }
        }

        /// <summary>
        /// Cập nhật bài tập (Mentor)
        /// </summary>
        [HttpPost("{classId}/sua/{assignmentId}")]
        [Authorize(Roles = UserRoles.Mentor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int classId, int assignmentId, UpdateAssignmentForm form)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ.";
                    return RedirectToAction("Sua", new { classId, assignmentId });
                }

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(form.Title), "Title");
                
                if (!string.IsNullOrEmpty(form.Description))
                    formData.Add(new StringContent(form.Description), "Description");
                
                formData.Add(new StringContent(form.DueDate.ToString("o")), "DueDate");
                formData.Add(new StringContent(form.RemoveAttachment.ToString()), "RemoveAttachment");

                if (form.AttachmentFile != null)
                {
                    var fileContent = new StreamContent(form.AttachmentFile.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        form.AttachmentFile.ContentType);
                    formData.Add(fileContent, "AttachmentFile", form.AttachmentFile.FileName);
                }

                var response = await _httpClient.PutAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{classId}/{assignmentId}"),
                    formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật bài tập thành công!";
                    return RedirectToAction("ChiTiet", new { classId, assignmentId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Không thể cập nhật bài tập: {errorContent}";
                    return RedirectToAction("Sua", new { classId, assignmentId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating assignment");
                TempData["Error"] = "Đã xảy ra lỗi khi cập nhật bài tập.";
                return RedirectToAction("Sua", new { classId, assignmentId });
            }
        }

        /// <summary>
        /// Xóa bài tập (Mentor)
        /// </summary>
        [HttpPost("{classId}/xoa/{assignmentId}")]
        [Authorize(Roles = UserRoles.Mentor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int classId, int assignmentId)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var response = await _httpClient.DeleteAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{classId}/{assignmentId}"));

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Xóa bài tập thành công!");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting assignment {AssignmentId}", assignmentId);
                return StatusCode(500, "Đã xảy ra lỗi khi xóa bài tập.");
            }
        }

        /// <summary>
        /// Nộp bài tập (Staff)
        /// </summary>
        [HttpPost("{assignmentId}/nop-bai")]
        [Authorize(Roles = UserRoles.Staff)]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> NopBai(int assignmentId, SubmitAssignmentForm form)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                if (form.File == null || form.File.Length <= 0)
                {
                    return BadRequest("Vui lòng chọn file để nộp.");
                }

                using var formData = new MultipartFormDataContent();
                
                var fileContent = new StreamContent(form.File.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    form.File.ContentType);
                formData.Add(fileContent, "File", form.File.FileName);

                if (!string.IsNullOrEmpty(form.Note))
                    formData.Add(new StringContent(form.Note), "Note");

                var response = await _httpClient.PostAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{assignmentId}/submissions"),
                    formData);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Nộp bài thành công!");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting assignment {AssignmentId}", assignmentId);
                return StatusCode(500, "Đã xảy ra lỗi khi nộp bài.");
            }
        }

        /// <summary>
        /// Danh sách bài nộp của assignment (Mentor)
        /// </summary>
        [HttpGet("{assignmentId}/danh-sach-bai-nop")]
        [Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> DanhSachBaiNop(int assignmentId)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                // Lấy thông tin bài tập
                var assignmentResponse = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{assignmentId}/submissions"));

                if (!assignmentResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không thể tải danh sách bài nộp.";
                    return RedirectToAction("CuaToi", "LopHoc");
                }

                var responseContent = await assignmentResponse.Content.ReadAsStringAsync();
                var submissions = JsonSerializer.Deserialize<List<AssignmentSubmissionSummaryDto>>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                    ?? new List<AssignmentSubmissionSummaryDto>();

                ViewBag.AssignmentId = assignmentId;

                return View(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting submissions for assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách bài nộp.";
                return RedirectToAction("CuaToi", "LopHoc");
            }
        }

        /// <summary>
        /// Chi tiết bài nộp (Mentor & Staff)
        /// </summary>
        [HttpGet("{assignmentId}/bai-nop/{submissionId}")]
        public async Task<IActionResult> ChiTietBaiNop(int assignmentId, int submissionId)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("DangNhap", "XacThuc");
                }

                var response = await _httpClient.GetAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{assignmentId}/submissions/{submissionId}"));

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy bài nộp.";
                    return RedirectToAction("CuaToi", "LopHoc");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var submission = JsonSerializer.Deserialize<AssignmentSubmissionDetailDto>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (submission == null)
                {
                    TempData["Error"] = "Không tìm thấy bài nộp.";
                    return RedirectToAction("CuaToi", "LopHoc");
                }

                ViewBag.IsMentor = User.IsInRole(UserRoles.Mentor);
                ViewBag.AssignmentId = assignmentId;

                return View(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting submission detail {SubmissionId}", submissionId);
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết bài nộp.";
                return RedirectToAction("CuaToi", "LopHoc");
            }
        }

        /// <summary>
        /// Chấm điểm bài nộp (Mentor)
        /// </summary>
        [HttpPost("{assignmentId}/bai-nop/{submissionId}/cham-diem")]
        [Authorize(Roles = UserRoles.Mentor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiem(int assignmentId, int submissionId, [FromBody] GradeSubmissionDto dto)
        {
            try
            {
                if (TokenHelpers.IsTokenExpired(_httpContextAccessor))
                {
                    return Unauthorized("Phiên đăng nhập đã hết hạn.");
                }

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(
                    Utilities.GetAbsoluteUrl($"api/assignment/{assignmentId}/submissions/{submissionId}/grade"),
                    content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Chấm điểm thành công!");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while grading submission {SubmissionId}", submissionId);
                return StatusCode(500, "Đã xảy ra lỗi khi chấm điểm.");
            }
        }
    }
}
