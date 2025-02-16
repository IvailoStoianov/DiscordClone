using DiscordClone.Data.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordClone.Data.Models
{
    public class Message
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.Message.ContentMaxLength)]
        public string Content { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid ChatRoomId { get; set; }

        [ForeignKey(nameof(ChatRoomId))]
        public virtual ChatRoom ChatRoom { get; set; } = null!;
        [Required]
        public bool IsDeleted { get; set; } = false;
        [Required]
        public bool IsEdited { get; set; } = false;
    }
}
