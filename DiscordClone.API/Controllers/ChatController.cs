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

        public ChatController(IChatService chatService, IUserService userService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
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
            var result = await _chatService.RemoveUserFromChatAsync(
                Guid.Parse(chatId), 
                Guid.Parse(userId), 
                userToRemoveId);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        /// <summary>
        /// Get all members of a specific chat room
        /// </summary>
        /// <param name="chatRoomId">The ID of the chat room</param>
        /// <returns>List of users in the chat room</returns>
        [HttpGet("{chatRoomId}/members")]
        public async Task<IActionResult> GetChatRoomMembers(string chatRoomId)
        {
            try
            {
                _logger.LogInformation($"Getting members for chat room: {chatRoomId}");

                // Verify chat room exists
                var chatRoom = await _chatService.GetChatByIdAsync(Guid.Parse(chatRoomId));
                if (chatRoom == null)
                {
                    _logger.LogWarning($"Chat room not found: {chatRoomId}");
                    return NotFound(new { message = "Chat room not found" });
                }

                // Get current user ID from claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authorized" });
                }

                // Verify user is a member of the chat room
                bool isMember = await _chatService.IsUserInChatRoomAsync(Guid.Parse(userId), Guid.Parse(chatRoomId));
                if (!isMember)
                {
                    _logger.LogWarning($"User {userId} is not a member of chat room {chatRoomId}");
                    return Forbid();
                }

                // Get all members of the chat room
                var members = await _chatService.GetChatRoomMembersAsync(Guid.Parse(chatRoomId));

                // Map to view models to avoid exposing sensitive information
                var memberViewModels = members.Select(m => new UserViewModel
                {
                    Id = m.Id,
                    Username = m.UserName,
                }).ToList();

                return Ok(memberViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting members for chat room {chatRoomId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
