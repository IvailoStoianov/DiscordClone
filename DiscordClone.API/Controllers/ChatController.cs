using Microsoft.AspNetCore.Mvc;
using DiscordClone.Services.Data.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DiscordClone.ViewModels;
using DiscordClone.ViewModels.ChatRoom;

namespace DiscordClone.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAllChatsForUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            var chats = await _chatService.GetAllChatsForUserAsync(Guid.Parse(userId));
            return Ok(chats);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetChat(string id)
        {
            var chat = await _chatService.GetChatByIdAsync(Guid.Parse(id));
            if (chat == null)
            {
                return NotFound();
            }
            return Ok(chat);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateChat([FromBody] ChatRoomInputModel chat)
        {
            // Add debugging
            Console.WriteLine($"Is Authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"Authentication Type: {User.Identity?.AuthenticationType}");
            
            var claims = User.Claims.ToList();
            Console.WriteLine($"Claims Count: {claims.Count}");
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                Console.WriteLine("UserId claim not found");
                return Unauthorized();
            }
            
            Console.WriteLine($"Found UserId: {userId}");
            Guid chatId = await _chatService.CreateChatAsync(chat, Guid.Parse(userId));
            return CreatedAtAction(nameof(GetChat), new { id = chatId.ToString() }, chat);
        }

        [HttpPost]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateChat([FromBody] ChatRoomViewModel chat)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.UpdateChatAsync(chat, Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPost]
        [Route("message")]
        public async Task<IActionResult> PostMessage([FromBody] MessageViewModel message)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var messageId = await _chatService.PostMessageAsync(message, Guid.Parse(userId));
            return CreatedAtAction(nameof(PostMessage), new { id = messageId }, message);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteChat(string chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.SoftDeleteChatAsync(Guid.Parse(chatId), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpDelete]
        [Route("message/{id}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.SoftDeleteMessageAsync(Guid.Parse(messageId), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPost]
        [Route("{id}/users/{userId}")]
        public async Task<IActionResult> AddUserToChat(string chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.AddUserToChatAsync(Guid.Parse(chatId), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromChat(string chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.RemoveUserFromChatAsync(Guid.Parse(chatId), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
