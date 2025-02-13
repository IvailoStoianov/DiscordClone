using Microsoft.AspNetCore.Mvc;

namespace DiscordClone.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetAllChats()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetChat(int id)
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateChat()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PostMessage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteChat(int id)
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddUserToChat(int id)
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RemoveUserFromChat(int id)
        {
            return View();
        }
    }
}
