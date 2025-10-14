using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Helpers;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class TrangChuController : Controller
    {

        public TrangChuController()
        {
          
        }


        public IActionResult Index()
        {
            ViewBag.Message = "Chào mừng đến với Hệ thống Đào tạo Nâng cao Năng lực!";
            return View();
        }

        public IActionResult GioiThieu()
        {
            ViewBag.Message = "Tìm hiểu về hệ thống đào tạo chuyên nghiệp của chúng tôi";
            return View();
        }

    }
}