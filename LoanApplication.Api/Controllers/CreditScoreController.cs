using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    public class CreditScoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
