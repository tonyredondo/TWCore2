using Microsoft.AspNetCore.Mvc;

namespace TWCore.Object.Api.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
