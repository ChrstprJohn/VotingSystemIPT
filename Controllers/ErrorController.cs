using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;

namespace VotingSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
