using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.ViewModels
{
    public class ChatRoomViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<MessageViewModel> Messages { get; set; } 
            = new List<MessageViewModel>();
        
        // Additional UI-specific properties
        public string NewMessageText { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public int UnreadMessageCount { get; set; }
        public DateTime LastActivityTime { get; set; } = DateTime.UtcNow;
    }
}
