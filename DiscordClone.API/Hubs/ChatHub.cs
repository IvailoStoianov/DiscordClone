using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace DiscordClone.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }
        public async Task JoinChatRoom(string chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
            _logger.LogInformation($"User {Context.User.Identity.Name} joined chat room {chatRoomId}");
        }

        public async Task LeaveChatRoom(string chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
            _logger.LogInformation($"User {Context.User.Identity.Name} left chat room {chatRoomId}");
        }

        public async Task SendMessageToGroup(string chatRoomId, string message)
        {
            _logger.LogInformation($"User {Context.User.Identity.Name} sent message to room {chatRoomId}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"User connected: {Context.ConnectionId}, {Context.User.Identity.Name}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"User disconnected: {Context.ConnectionId}, {Context.User.Identity.Name}");
            await base.OnDisconnectedAsync(exception);
        }
    }
} 