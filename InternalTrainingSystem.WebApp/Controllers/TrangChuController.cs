using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Helpers;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class TrangChuController : Controller
    {

        public TrangChuController()
        {
          
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Message = "Chào mừng đến với Hệ thống Đào tạo Nâng cao Năng lực!";
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("", "TrangChu");
        }

    }
}