using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ILogger<ClassController> _logger;

        public ClassController(IClassService classService, ICourseService courseService,
            IUserService userService, ILogger<ClassController> logger)
        {
            _classService = classService;
            _courseService = courseService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var classes = await _classService.GetClassesAsync();
                return View(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading classes");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách lớp học";
                return View(new List<ClassDto>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                // Load danh sách courses, mentors và staff
                var courses = await _courseService.GetAllCoursesAsync();
                var mentors = await _userService.GetMentors();
                var staff = await _userService.GetAllStaff();

                ViewBag.Courses = courses;
                ViewBag.Mentors = mentors;
                ViewBag.Staff = staff;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create class page");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo lớp học";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClasses([FromBody] CreateClassesDto createClassesDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdClasses = await _classService.CreateClassesAsync(createClassesDto);
                return Json(new { success = true, message = "Tạo lớp học thành công!", data = createdClasses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating classes");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            try
            {
                var classes = await _classService.GetClassesAsync();
                return Json(new { success = true, data = classes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classes");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách lớp học" });
            }
        }




    }
}
