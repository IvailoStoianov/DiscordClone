using DiscordClone.ViewModels;

namespace DiscordClone.Services.Data.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatRoomViewModel>> GetAllChatsAsync();
        Task<ChatRoomViewModel?> GetChatByIdAsync(Guid id);
        Task<Guid> CreateChatAsync(ChatRoomViewModel chat);
        Task<bool> UpdateChatAsync(ChatRoomViewModel chat);
        Task<bool> DeleteChatAsync(Guid id);
        Task<Guid> PostMessageAsync(MessageViewModel message);
        Task<bool> DeleteMessageAsync(Guid id);
        Task<bool> AddUserToChatAsync(Guid chatId, Guid userId);
        Task<bool> RemoveUserFromChatAsync(Guid chatId, Guid userId);
    }
}
