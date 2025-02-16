using Microsoft.AspNetCore.Mvc;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels;
using DiscordClone.ViewModels.ChatRoom;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> GetAllChats()
        {
            var chats = await _chatService.GetAllChatsAsync();
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
        public async Task<IActionResult> CreateChat(ChatRoomInputModel chat)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            Guid chatId = await _chatService.CreateChatAsync(chat, Guid.Parse(userId));
            return CreatedAtAction(nameof(GetChat), new { id = chatId.ToString() }, chat);
        }
        [HttpPost]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateChat(string id, ChatRoomViewModel chat)
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
        public async Task<IActionResult> PostMessage(MessageViewModel message)
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
        public async Task<IActionResult> DeleteChat(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.SoftDeleteChatAsync(Guid.Parse(id), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpDelete]
        [Route("message/{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.SoftDeleteMessageAsync(Guid.Parse(id), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpPost]
        [Route("{id}/users/{userId}")]
        public async Task<IActionResult> AddUserToChat(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.AddUserToChatAsync(Guid.Parse(id), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpDelete]
        [Route("{id}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromChat(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.RemoveUserFromChatAsync(Guid.Parse(id), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
