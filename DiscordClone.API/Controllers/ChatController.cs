using Microsoft.AspNetCore.Mvc;
using DiscordClone.Services.Data.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DiscordClone.ViewModels;
using DiscordClone.ViewModels.ChatRoom;
using DiscordClone.Data.Models;
using DiscordClone.Services.Data;
using Microsoft.Extensions.Logging;
using DiscordClone.ViewModels.User;
using Microsoft.AspNetCore.SignalR;
using DiscordClone.API.Hubs;
using DiscordClone.Data.Repository;

namespace DiscordClone.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly MessageRepository _messageRepository;

        public ChatController(
            IChatService chatService, 
            IUserService userService, 
            ILogger<ChatController> logger,
            IHubContext<ChatHub> chatHubContext,
            MessageRepository messageRepository)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
            _chatHubContext = chatHubContext;
            _messageRepository = messageRepository;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("CreateChat attempt with no user ID in claims");
                return Unauthorized();
            }
            
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
            
            try
            {
                // Post the message to the database
                var messageId = await _chatService.PostMessageAsync(message, Guid.Parse(userId));
                
                // Get the updated message with user information to broadcast
                var chat = await _chatService.GetChatByIdAsync(message.ChatRoomId);
                var newMessage = chat?.Messages.FirstOrDefault(m => m.Id == messageId);
                
                if (newMessage != null)
                {
                    // Send the message to all clients in this chat room group
                    await _chatHubContext.Clients.Group(message.ChatRoomId.ToString())
                        .SendAsync("ReceiveMessage", newMessage);
                }
                
                return CreatedAtAction(nameof(PostMessage), new { id = messageId }, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting message");
                return StatusCode(500, new { message = "Error posting message" });
            }
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
            
            try
            {
                // Get the full message details before deletion
                var message = await _messageRepository.GetByIdAsync(Guid.Parse(messageId));
                if (message == null)
                {
                    return NotFound();
                }
                
                // Store the chat room ID for SignalR notification
                var chatRoomId = message.ChatRoomId;
                
                // Delete the message
                var result = await _chatService.SoftDeleteMessageAsync(Guid.Parse(messageId), Guid.Parse(userId));
                if (!result)
                {
                    return NotFound();
                }
                
                // Notify all clients in the chat room about the deleted message
                await _chatHubContext.Clients.Group(chatRoomId.ToString())
                    .SendAsync("MessageDeleted", messageId);
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId}");
                return StatusCode(500, new { message = "Error deleting message" });
            }
        }


        [HttpPost]
        [Route("{chatId}/users/{username}")]
        public async Task<IActionResult> AddUserToChat(string chatId, string username)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _chatService.AddUserToChatAsync(Guid.Parse(chatId), Guid.Parse(userId), username);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }


        [HttpDelete]
        [Route("{chatId}/users/{userToRemoveId}")]
        public async Task<IActionResult> RemoveUserFromChat(string chatId, string userToRemoveId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            
            // Call the service method with the correct number of parameters
            var result = await _chatService.RemoveUserFromChatAsync(
                Guid.Parse(chatId),
                Guid.Parse(userId));
                
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpGet("{chatRoomId}/members")]
        public async Task<IActionResult> GetChatRoomMembers(string chatRoomId)
        {
            try
            {
                var members = await _chatService.GetChatRoomMembersAsync(Guid.Parse(chatRoomId));
                
                var userViewModels = members.Select(m => new UserViewModel
                {
                    Id = m.Id,
                    Username = m.UserName
                });
                
                return Ok(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting members for chat room {chatRoomId}");
                return StatusCode(500, new { message = "Error retrieving chat room members" });
            }
        }
    }
}
