using Microsoft.EntityFrameworkCore;
using Chrika.Api.Models;

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
        public DbSet<Page> Pages { get; set; }
        public DbSet<PagePost> PagePosts { get; set; }
        public DbSet<AdCampaign> AdCampaigns { get; set; }
        public DbSet<Transaction> Transactions { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === کلیلە تێکەڵەکان (Composite Keys) ===
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.PostId, l.UserId });

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            // === پەیوەندی Follow ===
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // === پەیوەندی Notification ===
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TriggeredByUser)
                .WithMany()
                .HasForeignKey(n => n.TriggeredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================================================
            // === START: چارەسەری هەڵە نوێیەکە لێرەدایە ===
            // ======================================================

            // 1. پەیوەندی نێوان Post و Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post) // هەر کۆمێنتێک یەک پۆستی هەیە
                .WithMany(p => p.Comments) // هەر پۆستێک چەندین کۆمێنتی هەیە
                .HasForeignKey(c => c.PostId) // کلیلی بیانی ستوونی PostId ـە
                .OnDelete(DeleteBehavior.Cascade); // ئەگەر پۆستێک سڕایەوە، هەموو کۆمێنتەکانیشی بسڕەوە

            // 2. پەیوەندی نێوان Post و Like
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post) // هەر لایکێک بۆ یەک پۆستە
                .WithMany(p => p.Likes) // هەر پۆستێک چەندین لایکی هەیە
                .HasForeignKey(l => l.PostId) // کلیلی بیانی ستوونی PostId ـە
                .OnDelete(DeleteBehavior.Cascade); // ئەگەر پۆستێک سڕایەوە، هەموو لایکەکانیشی بسڕەوە

          modelBuilder.Entity<AdCampaign>()
                .OwnsOne(c => c.Audience); //بۆ سپۆنسەرە 

            // ======================================================
            // === END: کۆتایی چارەسەرەکە ===
            // ======================================================
        }
    }
}
