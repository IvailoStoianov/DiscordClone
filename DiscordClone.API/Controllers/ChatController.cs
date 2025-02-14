using Microsoft.AspNetCore.Mvc;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels;
namespace DiscordClone.API.Controllers
{
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
        public async Task<IActionResult> CreateChat(ChatRoomViewModel chat)
        {
            Guid chatId = await _chatService.CreateChatAsync(chat);
            chat.Id = chatId;
            return CreatedAtAction(nameof(GetChat), new { id = chatId.ToString() }, chat);
        }
        [HttpPost]
        [Route("message")]
        public async Task<IActionResult> PostMessage(MessageViewModel message)
        {
            var messageId = await _chatService.PostMessageAsync(message);
            return CreatedAtAction(nameof(PostMessage), new { id = messageId }, message);
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteChat(string id)
        {
            var result = await _chatService.DeleteChatAsync(Guid.Parse(id));
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
            var result = await _chatService.DeleteMessageAsync(Guid.Parse(id));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpPost]
        [Route("{id}/users/{userId}")]
        public async Task<IActionResult> AddUserToChat(string id, string userId)
        {
            var result = await _chatService.AddUserToChatAsync(Guid.Parse(id), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpDelete]
        [Route("{id}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromChat(string id, string userId)
        {
            var result = await _chatService.RemoveUserFromChatAsync(Guid.Parse(id), Guid.Parse(userId));
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
