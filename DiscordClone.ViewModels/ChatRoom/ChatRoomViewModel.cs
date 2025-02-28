using DiscordClone.ViewModels;

public class ChatRoomViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<MessageViewModel> Messages { get; set; } 
        = new HashSet<MessageViewModel>();
} 