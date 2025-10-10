using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Implement;
using InternalTrainingSystem.WebApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class QuanLyTrucTiepController : Controller
    {
        private readonly ILogger<QuanLyTrucTiepController> _logger;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public QuanLyTrucTiepController(ILogger<QuanLyTrucTiepController> logger, 
            IUserService userService, INotificationService notificationService)
        {
            _logger = logger;
            _userService = userService;
            _notificationService = notificationService;
        }
        public IActionResult TrangChuQuanLyTrucTiep()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietKhoaHoc(int courseId)
        {
            try
            {
                var (isSent, sentAt) = await _notificationService.CheckNotificationStatusAsync(
                    courseId,
                    NotificationType.StaffConfirm
                );

                ViewBag.CourseId = courseId;
                ViewBag.IsNotificationSent = isSent;
                ViewBag.SentAt = sentAt;

                var staffList = await _userService.GetUserRoleEligibleStaff(courseId);
                return View(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi lấy danh sách staff chưa có certificate của course {CourseId}",
                    courseId);

                ViewBag.CourseId = courseId;
                ViewBag.IsNotificationSent = false;
                ViewBag.SentAt = null;

                return View(new List<EligibleStaffResponse>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> NotifyEligibleUsers(int courseId)
        {
            var result = await _notificationService.NotifyEligibleUsersAsync(courseId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction("ChiTietKhoaHoc", new { courseId });
        }
    }
}
