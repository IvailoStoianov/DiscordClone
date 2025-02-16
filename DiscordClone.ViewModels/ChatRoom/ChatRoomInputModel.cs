using DiscordClone.Data.Models.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.ViewModels.ChatRoom
{
    public class ChatRoomInputModel
    {
        [Required]
        [MaxLength(ValidationConstants.ChatRoom.NameMaxLength)]
        [MinLength(ValidationConstants.ChatRoom.NameMinLength)]
        public string Name { get; set; } = null!;

    }
}
