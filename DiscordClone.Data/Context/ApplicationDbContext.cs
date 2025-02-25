using DiscordClone.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public new DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
             
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
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(m => m.ChatRoom)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ChatRoom relationships
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(c => c.Owner)
                    .WithMany()
                    .HasForeignKey(c => c.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ChatRoomUser join entity
            modelBuilder.Entity<ChatRoomUser>(entity =>
            {
                entity.HasKey(e => new { e.ChatRoomId, e.UserId });
                
                entity.HasOne(cru => cru.ChatRoom)
                    .WithMany()
                    .HasForeignKey(cru => cru.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(cru => cru.User)
                    .WithMany()
                    .HasForeignKey(cru => cru.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.ToTable("ChatRoomUsers");
            });

            // Configure many-to-many relationship between ChatRoom and User
            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatRooms)
                .WithMany(c => c.Users)
                .UsingEntity<ChatRoomUser>();
        }
    }
}
