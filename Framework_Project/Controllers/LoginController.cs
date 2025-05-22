using Microsoft.AspNetCore.Mvc;

namespace Framework_Project.Controllers
{
    public class LoginController : Controller 
    {
        public IActionResult Index() 
        {
            return View(); 
        }
    }
}
