using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.WebApp.Helpers;

namespace InternalTrainingSystem.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Ví dụ action với tên PascalCase sẽ được chuyển thành kebab-case
        // TrangChu sẽ thành trang-chu trong URL
        public IActionResult TrangChu()
        {
            ViewBag.Message = "Đây là trang chủ với URL kebab-case!";
            return View();
        }

        public IActionResult GioiThieu()
        {
            ViewBag.Message = "Trang giới thiệu - URL: Home/gioi-thieu";
            return View();
        }

        public IActionResult LienHe()
        {
            ViewBag.Message = "Trang liên hệ - URL: Home/lien-he";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        // API method để test URL generation
        [HttpGet]
        public IActionResult GetKebabUrls()
        {
            var urls = new
            {
                TrangChu = Utilities.GetKebabUrl("Home", "TrangChu"),
                GioiThieu = Utilities.GetKebabUrl("Home", "GioiThieu"), 
                LienHe = Utilities.GetKebabUrl("Home", "LienHe"),
                Privacy = Utilities.GetKebabUrl("Home", "Privacy")
            };

            return Json(urls);
        }
    }
}