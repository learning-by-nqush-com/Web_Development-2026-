using Microsoft.AspNetCore.Mvc;

namespace Staybnb.Controllers
{
    public class HostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
