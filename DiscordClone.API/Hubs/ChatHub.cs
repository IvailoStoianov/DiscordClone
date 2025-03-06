using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using DiscordClone.ViewModels.ChatRoom;

namespace DiscordClone.API.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat functionality
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Adds the current connection to a chat room group
        /// </summary>
        public async Task JoinChatRoom(string chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
            _logger.LogDebug("User {Username} joined chat room {ChatRoomId}", 
                Context.User.Identity?.Name, chatRoomId);
        }

        /// <summary>
        /// Removes the current connection from a chat room group
        /// </summary>
        public async Task LeaveChatRoom(string chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
            _logger.LogDebug("User {Username} left chat room {ChatRoomId}", 
                Context.User.Identity?.Name, chatRoomId);
        }
        
        /// <summary>
        /// Logs when a connection is established
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogDebug("User connected: {ConnectionId}, {Username}", 
                Context.ConnectionId, Context.User.Identity?.Name);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Logs when a connection is terminated
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogDebug("User disconnected: {ConnectionId}, {Username}", 
                Context.ConnectionId, Context.User.Identity?.Name);
            await base.OnDisconnectedAsync(exception);
        }
    }
} 