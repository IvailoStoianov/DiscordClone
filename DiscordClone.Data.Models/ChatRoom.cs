using DiscordClone.Data.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordClone.Data.Models
{
    public class ChatRoom
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(ValidationConstants.ChatRoom.NameMaxLength)]

        public string Name { get; set; } = null!;

        [MaxLength(ValidationConstants.ChatRoom.DescriptionMaxLength)]

        public string Description { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual User Owner { get; set; } = null!;

        public virtual ICollection<Message> Messages { get; set; } 
            = new HashSet<Message>();

        public virtual ICollection<User> Users { get; set; } 
            = new HashSet<User>();

    }
}