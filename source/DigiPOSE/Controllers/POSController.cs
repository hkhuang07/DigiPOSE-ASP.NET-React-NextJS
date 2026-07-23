using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiPOSE.Controllers
{
    [Authorize(Policy = "POS.Order.Create")]
    public class POSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
