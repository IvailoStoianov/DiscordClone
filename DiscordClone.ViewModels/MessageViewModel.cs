using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.ViewModels
{
    public class MessageViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid ChatRoomId { get; set; }

        public bool IsEdited { get; set; }
        public bool IsOwnMessage { get; set; }
        public string? FormattedTimestamp { get; set; }

        
    }
}
