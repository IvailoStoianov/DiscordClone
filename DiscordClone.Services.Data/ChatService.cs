using DiscordClone.Data.Models;
using DiscordClone.Data.Repository;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels;
using DiscordClone.ViewModels.ChatRoom;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<ChatRoomViewModel>> GetAllChatsForUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var chats = await _chatRoomRepository.GetChatRoomsForUserAsync(userId);
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
                Messages = chat.Messages.Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    Content = m.Content,
                    UserId = m.UserId,
                    ChatRoomId = m.ChatRoomId,
                })
            } : null;
        }
        public async Task<Guid> CreateChatAsync(ChatRoomInputModel chat, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var newChat = new ChatRoom
            {
                Name = chat.Name,
                OwnerId = userId,
                Users = new List<User> { user }
            };
            await _chatRoomRepository.AddAsync(newChat);
            return newChat.Id;
        }

        public async Task<bool> UpdateChatAsync(ChatRoomViewModel chat, Guid userId)
        {
            var existingChat = await _chatRoomRepository.GetByIdAsync(chat.Id);
            if (existingChat == null)
            {
                return false;
            }
            if (existingChat.OwnerId != userId)
            {
                return false;
            }
            existingChat.Name = chat.Name;
            await _chatRoomRepository.UpdateAsync(existingChat);
            return true;
        }

        public async Task<bool> SoftDeleteChatAsync(Guid id, Guid userId)
        {
            var chat = await _chatRoomRepository.GetByIdAsync(id);
            if (chat == null)
            {
                return false;
            }
            if (chat.OwnerId != userId)
            {
                return false;
            }
            await _chatRoomRepository.SoftDeleteAsync(id);
            return true;
        }

        public async Task<Guid> PostMessageAsync(MessageViewModel message, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var chat = await _chatRoomRepository.GetByIdAsync(message.ChatRoomId);
            if (chat == null)
            {
                throw new ArgumentException("Chat not found");
            }
            if (chat.Users.FirstOrDefault(user) == null)
            {
                throw new ArgumentException("User not in chat");
            }
            var newMessage = new Message
            {
                Content = message.Content,
                ChatRoomId = message.ChatRoomId,
                UserId = userId,  
            };
            await _messageRepository.AddAsync(newMessage);
            return newMessage.Id;
        }

        public async Task<bool> SoftDeleteMessageAsync(Guid id, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var message = await _messageRepository.GetByIdAsync(id);
            if (message == null)
            {
                return false;
            }
            if (message.UserId != userId)
            {
                return false;
            }
            await _messageRepository.SoftDeleteAsync(id);
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
            if(chat.Users.FirstOrDefault(user) != null)
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
            if (chat.Users.FirstOrDefault(user) == null)
            {
                return false;
            }
            chat.Users.Remove(user);
            await _chatRoomRepository.UpdateAsync(chat);
            return true;
        }


        public async Task<bool> IsUserInChatRoomAsync(Guid userId, Guid chatRoomId)
        {
            return await _chatRoomRepository.IsUserInChatRoomAsync(userId, chatRoomId);
        }

        public async Task<IEnumerable<User>> GetChatRoomMembersAsync(Guid chatRoomId)
        {
            return await _chatRoomRepository.GetChatRoomMembersAsync(chatRoomId);
        }
    }
}
