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
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl($"api/classes/{id}"));
                
                ClassDto? classDetail = null;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    classDetail = JsonSerializer.Deserialize<ClassDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "Không tìm thấy lớp học.";
                    return RedirectToAction("Index");
                }
                else
                {
                    var allClasses = new List<ClassDto>();
                    classDetail = allClasses.FirstOrDefault(c => c.ClassId == id);
                }

                if (classDetail == null)
                {
                    TempData["Error"] = "Không tìm thấy lớp học.";
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

        [HttpGet("tao-lop")]
        public async Task<IActionResult> TaoLop()
        {
            try
            {
                var model = new CreateClassViewModel
                {
                    Courses = await GetCoursesAsync(),
                    Mentors = await GetMentorsAsync(),
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(3),
                    Capacity = 20,
                    Schedule = new List<ClassScheduleItem>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create class page");
                TempData["Error"] = "Đã xảy ra lỗi khi tải trang tạo lớp học.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("tao-lop")]
        public async Task<IActionResult> TaoLop(CreateClassViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Courses = await GetCoursesAsync();
                    model.Mentors = await GetMentorsAsync();
                    return View(model);
                }

                // Validate dates
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
                    model.Courses = await GetCoursesAsync();
                    model.Mentors = await GetMentorsAsync();
                    return View(model);
                }

                // Call API to create class
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Utilities.GetAbsoluteUrl("api/classes"), content);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Tạo lớp học thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = !string.IsNullOrEmpty(errorContent) ? errorContent.Trim('"') : "Không thể tạo lớp học. Vui lòng thử lại.";
                    TempData["Error"] = errorMessage;
                    model.Courses = await GetCoursesAsync();
                    model.Mentors = await GetMentorsAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating class");
                TempData["Error"] = "Đã xảy ra lỗi khi tạo lớp học.";
                model.Courses = await GetCoursesAsync();
                model.Mentors = await GetMentorsAsync();
                return View(model);
            }
        }

        private async Task<List<CourseDto>> GetCoursesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/courses"));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var courses = JsonSerializer.Deserialize<List<CourseDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return courses ?? GetSampleCourses();
                }
                else
                {
                    _logger.LogWarning("Failed to get courses from API. Status: {StatusCode}", response.StatusCode);
                    return GetSampleCourses();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses from API");
                return GetSampleCourses();
            }
        }

        private async Task<List<MentorResponse>> GetMentorsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Utilities.GetAbsoluteUrl("api/mentors"));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var mentors = JsonSerializer.Deserialize<List<MentorResponse>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return mentors ?? GetSampleMentors();
                }
                else
                {
                    _logger.LogWarning("Failed to get mentors from API. Status: {StatusCode}", response.StatusCode);
                    return GetSampleMentors();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting mentors from API");
                return GetSampleMentors();
            }
        }

        private List<CourseDto> GetSampleCourses()
        {
            return new List<CourseDto>
            {
                new CourseDto { CourseId = 101, CourseName = "Lập Trình C# Cơ Bản", Level = "Beginner" },
                new CourseDto { CourseId = 102, CourseName = "ASP.NET Core Web API Development", Level = "Intermediate" },
                new CourseDto { CourseId = 103, CourseName = "React JS Frontend Development", Level = "Intermediate" },
                new CourseDto { CourseId = 104, CourseName = "SQL Server Database Administration", Level = "Beginner" },
                new CourseDto { CourseId = 105, CourseName = "Angular Framework Development", Level = "Advanced" },
                new CourseDto { CourseId = 106, CourseName = "Python Django Web Development", Level = "Intermediate" },
                new CourseDto { CourseId = 107, CourseName = "Java Spring Boot Development", Level = "Advanced" },
                new CourseDto { CourseId = 108, CourseName = "DevOps & Continuous Integration/Deployment", Level = "Advanced" }
            };
        }

        private List<MentorResponse> GetSampleMentors()
        {
            return new List<MentorResponse>
            {
                new MentorResponse { Id = "M001", FullName = "Nguyễn Văn Nam", Email = "nam.nguyen@fpt.edu.vn" },
                new MentorResponse { Id = "M002", FullName = "Trần Văn Hùng", Email = "hung.tran@fpt.edu.vn" },
                new MentorResponse { Id = "M003", FullName = "Lê Thị Hương", Email = "huong.le@fpt.edu.vn" },
                new MentorResponse { Id = "M004", FullName = "Phạm Văn Đức", Email = "duc.pham@fpt.edu.vn" },
                new MentorResponse { Id = "M005", FullName = "Võ Văn Tài", Email = "tai.vo@fpt.edu.vn" },
                new MentorResponse { Id = "M006", FullName = "Bùi Văn Nghĩa", Email = "nghia.bui@fpt.edu.vn" },
                new MentorResponse { Id = "M007", FullName = "Ngô Thị Phương", Email = "phuong.ngo@fpt.edu.vn" },
                new MentorResponse { Id = "M008", FullName = "Hà Văn Quý", Email = "quy.ha@fpt.edu.vn" }
            };
        }

       
    }
}
