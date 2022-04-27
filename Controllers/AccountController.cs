using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using System.Diagnostics;

namespace SV2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        
        [TempData]
        public string StatusMessage { get; set; }

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Manage()
        {
            User? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View(user);
        }

        public async Task<IActionResult> ViewAPIKey()
        {
            User? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View((object)user.Api_Key);
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("svid");
            return Redirect("/");
        }

        public IActionResult Entered()
        {
            string svid = UserManager.GetSvidFromSession(HttpContext);
            Console.WriteLine(HttpContext.Session.GetString("code"));
            HttpContext.Response.Cookies.Append("svid", svid);
            return Redirect("/");
        }

        public IActionResult Login()
        {
            string Code = UserManager.GetCode(HttpContext);
            Console.WriteLine(HttpContext.Session.Id);
            return View((Object)Code);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}