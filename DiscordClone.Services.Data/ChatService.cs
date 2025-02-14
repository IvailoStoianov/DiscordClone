using DiscordClone.Data.Models;
using DiscordClone.Data.Repository;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels;
namespace DiscordClone.Services.Data
{
    public class ChatService : IChatService
    {
        private readonly UserRepository _userRepository;
        private readonly ChatRoomRepository _chatRoomRepository;
        private readonly MessageRepository _messageRepository;

        public ChatService(
            UserRepository userRepository,
            ChatRoomRepository chatRoomRepository,
            MessageRepository messageRepository
        )
        {
            _userRepository = userRepository;
            _chatRoomRepository = chatRoomRepository;
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<ChatRoomViewModel>> GetAllChatsAsync()
        {
            var chats = await _chatRoomRepository.GetAllAsync();
            return chats.Select(chat => new ChatRoomViewModel
            {
                Id = chat.Id,
                Name = chat.Name,
            });
        }

        public async Task<ChatRoomViewModel?> GetChatByIdAsync(Guid id)
        {
            var chat = await _chatRoomRepository.GetByIdAsync(id);
            return chat != null ? new ChatRoomViewModel
            {
                Id = chat.Id,
                Name = chat.Name,
            } : null;
        }

        public async Task<Guid> CreateChatAsync(ChatRoomViewModel chat)
        {
            var newChat = new ChatRoom
            {
                Name = chat.Name,
            };
            await _chatRoomRepository.AddAsync(newChat);
            return newChat.Id;
        }

        public async Task<bool> UpdateChatAsync(ChatRoomViewModel chat)
        {
            var existingChat = await _chatRoomRepository.GetByIdAsync(chat.Id);
            if (existingChat == null)
            {
                return false;
            }
            existingChat.Name = chat.Name;
            await _chatRoomRepository.UpdateAsync(existingChat);
            return true;
        }

        public async Task<bool> DeleteChatAsync(Guid id)
        {
            var chat = await _chatRoomRepository.GetByIdAsync(id);
            if (chat == null)
            {
                return false;
            }
            await _chatRoomRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Guid> PostMessageAsync(MessageViewModel message)
        {
            var newMessage = new Message
            {
                Content = message.Content,
                ChatRoomId = message.ChatRoomId,
                UserId = message.UserId,
                Timestamp = message.Timestamp,
            };
            await _messageRepository.AddAsync(newMessage);
            return newMessage.Id;

        }

        public async Task<bool> DeleteMessageAsync(Guid id)
        {
            var message = await _messageRepository.GetByIdAsync(id);
            if (message == null)
            {
                return false;
            }
            await _messageRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> AddUserToChatAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRoomRepository.GetByIdAsync(chatId);
            if (chat == null)
            {
                return false;
            }
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            chat.Users.Add(user);
            await _chatRoomRepository.UpdateAsync(chat);
            return true;
        }

        public async Task<bool> RemoveUserFromChatAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRoomRepository.GetByIdAsync(chatId);
            if (chat == null)
            {
                return false;
            }
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            chat.Users.Remove(user);
            await _chatRoomRepository.UpdateAsync(chat);
            return true;
        }
        
        
    }
}
