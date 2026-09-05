using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VotingSystem.Controllers
{
    [Authorize(Roles = "PartylistLeader,Administrator")]
    public class PartylistsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
