using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VotingSystem.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }

        public IActionResult Voters()
        {
            ViewData["ActivePage"] = "Voters";
            return View();
        }

        public IActionResult Elections()
        {
            ViewData["ActivePage"] = "Elections";
            return View();
        }

        public IActionResult ElectionDetail(string? id, string? tab)
        {
            ViewData["ActivePage"] = "Elections";
            ViewData["ElectionId"] = id ?? "1";
            ViewData["ActiveTab"] = !string.IsNullOrEmpty(tab) ? tab.ToLower() : "overview";
            return View();
        }

        public IActionResult Settings()
        {
            ViewData["ActivePage"] = "Settings";
            return View();
        }
    }
}
