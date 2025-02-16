using DiscordClone.Data.Models.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.ViewModels.User
{
    public class LoginInputModel
    {
        [Required]
        [MaxLength(ValidationConstants.User.UsernameMaxLength)]
        [MinLength(ValidationConstants.User.UsernameMinLength)]
        public string Username { get; set; } = string.Empty;
    }
}
