using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading courses");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách khóa học";
                return View(new List<CourseDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Json(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách khóa học" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCoursesByIdentifiers([FromBody] GetCoursesByIdentifiersRequest request)
        {
            try
            {
                if (request?.Identifiers == null || !request.Identifiers.Any())
                {
                    return Json(new { success = false, message = "Danh sách định danh không được để trống" });
                }

                var courses = await _courseService.GetCoursesByIdentifiersAsync(request.Identifiers);
                return Json(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses by identifiers");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách khóa học" });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học";
                    return RedirectToAction(nameof(Index));
                }
                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading course details for id {id}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết khóa học";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
