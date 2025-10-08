using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassController> _logger;

        public ClassController(IClassService classService, ILogger<ClassController> logger)
        {
            _classService = classService;
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

        public IActionResult Create()
        {
            return View();
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
