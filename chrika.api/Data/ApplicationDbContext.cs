using Microsoft.EntityFrameworkCore;
using Chrika.Api.Models; // یان Chrika.Api.Entities

namespace Chrika.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Notification> Notifications { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === ڕاستکراوە: ڕێکخستنی Composite Keys ===

            // 1. کلیل بۆ خشتەی Like
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.PostId, l.UserId }); // تێکەڵەیەک لە PostId و UserId

            // 2. کلیل بۆ خشتەی Follow
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId }); // تێکەڵەیەک لە FollowerId و FollowingId

            // === ڕاستکراوە: ڕێکخستنی پەیوەندی Follow ===
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followings) // بەکارهێنانی ناوی ڕاستکراوە
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict); // گرنگە بۆ voorkomingی کێشەی سڕینەوە

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers) // بەکارهێنانی ناوی ڕاستکراوە
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);


            // === زیادکردنی ڕێکخستن بۆ Notification ===
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany() // Userێک دەتوانێت چەندین Notificationـی هەبێت
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict); // ڕێگە نادەین User بسڕێتەوە ئەگەر Notificationـی هەبێت

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TriggeredByUser)
                .WithMany()
                .HasForeignKey(n => n.TriggeredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
