namespace DiscordClone.ViewModels.Constants
{
    public static class ValidationConstants
    {
        public static class ChatRoom
        {
            public const int NameMaxLength = 100;
            public const int NameMinLength = 3;
        }

        public static class Message
        {
            public const int ContentMaxLength = 2000;
            public const int ContentMinLength = 1;
        }
    }
} 