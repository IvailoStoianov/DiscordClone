﻿using Microsoft.AspNetCore.Mvc;
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
                var messageId = await _chatService.PostMessageAsync(message, Guid.Parse(userId));
                
                var chat = await _chatService.GetChatByIdAsync(message.ChatRoomId);
                var newMessage = chat?.Messages.FirstOrDefault(m => m.Id == messageId);
                
                if (newMessage != null)
                {
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
                var message = await _messageRepository.GetByIdAsync(Guid.Parse(messageId));
                if (message == null)
                {
                    return NotFound();
                }
                
                var chatRoomId = message.ChatRoomId;
                
                var result = await _chatService.SoftDeleteMessageAsync(Guid.Parse(messageId), Guid.Parse(userId));
                if (!result)
                {
                    return NotFound();
                }
                
                await _chatHubContext.Clients.Group(chatRoomId.ToString())
                    .SendAsync("MessageDeleted", messageId);
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
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
            
            try
            {
                var result = await _chatService.AddUserToChatAsync(Guid.Parse(chatId), Guid.Parse(userId), username);
                if (!result)
                {
                    return NotFound();
                }
                
                var addedUser = await _userService.GetUserByUsernameAsync(username);
                if (addedUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                
                var chatRoom = await _chatService.GetChatByIdAsync(Guid.Parse(chatId));
                if (chatRoom == null)
                {
                    return NotFound(new { message = "Chat room not found" });
                }
                
                await _chatHubContext.Clients.User(addedUser.Id.ToString())
                    .SendAsync("UserAddedToChat", chatRoom);
                
                _logger.LogDebug("User {Username} (ID: {UserId}) added to chat {ChatRoomId}", 
                    username, addedUser.Id, chatId);
                
                await _chatHubContext.Clients.Group(chatId)
                    .SendAsync("UserJoinedRoom", new { userId = addedUser.Id, username = addedUser.UserName, roomId = chatId });
                
                return Ok(new { message = $"User {username} added to chat room" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {Username} to chat {ChatRoomId}", username, chatId);
                return StatusCode(500, new { message = "Error adding user to chat" });
            }
        }

        [HttpDelete]
        [Route("{chatId}/users/{username}")]
        public async Task<IActionResult> RemoveUserFromChat(string chatId, string username)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            
            try
            {
                var removedUser = await _userService.GetUserByUsernameAsync(username);
                if (removedUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                
                var result = await _chatService.RemoveUserFromChatAsync(
                    Guid.Parse(chatId),
                    removedUser.Id);
                    
                if (!result)
                {
                    return NotFound();
                }
                
                await _chatHubContext.Clients.User(removedUser.Id.ToString())
                    .SendAsync("UserRemovedFromChat", chatId);
                
                _logger.LogDebug("User {Username} (ID: {UserId}) removed from chat {ChatRoomId}", 
                    username, removedUser.Id, chatId);
                
                await _chatHubContext.Clients.Group(chatId)
                    .SendAsync("UserLeftRoom", new { userId = removedUser.Id, username = removedUser.UserName, roomId = chatId });
                
                return Ok(new { message = $"User {username} removed from chat room" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {Username} from chat {ChatRoomId}", username, chatId);
                return StatusCode(500, new { message = "Error removing user from chat" });
            }
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
                _logger.LogError(ex, "Error getting members for chat room {ChatRoomId}", chatRoomId);
                return StatusCode(500, new { message = "Error retrieving chat room members" });
            }
        }
    }
}
