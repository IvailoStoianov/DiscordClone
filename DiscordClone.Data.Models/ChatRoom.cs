using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
