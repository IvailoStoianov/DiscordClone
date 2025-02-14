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
        public string UserAvatarUrl { get; set; } = string.Empty;

        public Guid ChatRoomId { get; set; }

        public bool IsEdited { get; set; }
        public bool IsOwnMessage { get; set; }
        public bool IsRead { get; set; }
        public string FormattedTimestamp => FormatTimestamp();

        private string FormatTimestamp()
        {
            var timeDiff = DateTime.UtcNow - Timestamp;
            
            if (timeDiff.TotalMinutes < 1)
                return "Just now";
            if (timeDiff.TotalHours < 1)
                return $"{(int)timeDiff.TotalMinutes}m ago";
            if (timeDiff.TotalDays < 1)
                return $"{(int)timeDiff.TotalHours}h ago";
            if (timeDiff.TotalDays < 7)
                return $"{(int)timeDiff.TotalDays}d ago";
            
            return Timestamp.ToString("MMM dd, yyyy");
        }
    }
}
