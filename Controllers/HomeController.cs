using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VotingSystem.Models;

namespace VotingSystem.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        [Route("index")]
        [Route("home")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}