using DiscordClone.Data.Models.Constants;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DiscordClone.Data.Models
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            this.Id = Guid.NewGuid();
        }

        [Required]
        [MaxLength(ValidationConstants.User.UsernameMaxLength)]
        public override string UserName { get; set; } = null!;
        [Required]
        public bool IsDeleted { get; set; } = false;


        public virtual ICollection<Message> Messages { get; set; } = new HashSet<Message>();
        
        public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new HashSet<ChatRoom>();
    }
}
