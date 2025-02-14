using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Models
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            this.Id = Guid.NewGuid();
        }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatRoom> Chats { get; set; } = new List<ChatRoom>();
    }
}
