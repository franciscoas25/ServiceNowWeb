using Microsoft.AspNetCore.Mvc;

namespace ServiceNowWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadFile()
        {
            return View();
        }
    }
}
