using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Models
{
    public class ChatRoomUser
    {
        [Required]
        public Guid ChatRoomId { get; set; }

        [ForeignKey(nameof(ChatRoomId))]
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
