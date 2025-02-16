namespace DiscordClone.Data.Models.Constants
{
    public static class ValidationConstants
    {
        public static class User
        {
            public const int UsernameMaxLength = 50;
            public const int UsernameMinLength = 3;
        }

        public static class ChatRoom 
        {
            public const int NameMaxLength = 100;
            public const int NameMinLength = 3;
            public const int DescriptionMaxLength = 500;
        }

        public static class Message
        {
            public const int ContentMaxLength = 2000;
            public const int ContentMinLength = 1;
        }
    }
} 