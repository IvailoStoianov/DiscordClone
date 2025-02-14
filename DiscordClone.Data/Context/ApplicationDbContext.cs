using DiscordClone.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<User,IdentityRole<Guid> ,Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public new DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Message relationships
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(m => m.User)
                    .WithMany(u => u.Messages)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

                entity.HasOne(m => m.ChatRoom)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ChatRoom relationships
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasMany(c => c.Users)
                    .WithMany(u => u.Chats)
                    .UsingEntity<Dictionary<string, object>>(
                        "ChatRoomUser",
                        j => j
                            .HasOne<User>()
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<ChatRoom>()
                            .WithMany()
                            .HasForeignKey("ChatRoomId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("ChatRoomId", "UserId");
                            j.ToTable("ChatRoomUser");
                        });
            });
        }
    }
}
