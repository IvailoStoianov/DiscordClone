using DiscordClone.ViewModels;
using DiscordClone.ViewModels.ChatRoom;

namespace DiscordClone.Services.Data.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatRoomViewModel>> GetAllChatsAsync();
        Task<ChatRoomViewModel?> GetChatByIdAsync(Guid id);
        Task<Guid> CreateChatAsync(ChatRoomInputModel chat, Guid userId);
        Task<bool> UpdateChatAsync(ChatRoomViewModel chat, Guid userId);
        Task<bool> SoftDeleteChatAsync(Guid id, Guid userId);
        Task<Guid> PostMessageAsync(MessageViewModel message, Guid userId);
        Task<bool> SoftDeleteMessageAsync(Guid id, Guid userId);
        Task<bool> AddUserToChatAsync(Guid chatId, Guid userId);
        Task<bool> RemoveUserFromChatAsync(Guid chatId, Guid userId);
    }
}
