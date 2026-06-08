using Microsoft.AspNetCore.Mvc;

namespace Staybnb.Controllers
{
    public class GuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
