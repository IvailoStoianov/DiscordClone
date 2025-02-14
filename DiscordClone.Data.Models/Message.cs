using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public Guid UserId { get; set; }
        public User? User { get; set; } = null!;

        public Guid ChatRoomId { get; set; }
        public ChatRoom? ChatRoom { get; set; } = null!;
    }
}
