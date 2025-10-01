using Microsoft.EntityFrameworkCore;
using Chrika.Api.Models;

namespace Chrika.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet ـە کۆنەکان
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
        public DbSet<AdInteraction> AdInteractions { get; set; }
        public DbSet<Share> Shares { get; set; }

        // === DbSet ـە نوێیەکانی سیستەمی گرووپ ===
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupFollower> GroupFollowers { get; set; }
        public DbSet<GroupPost> GroupPosts { get; set; }
        public DbSet<GroupJoinRequest> GroupJoinRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // --- پێناسەکردنی کلیلە تێکەڵەکان (Composite Keys) ---
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.PostId, l.UserId });

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            // === زیادکرا: کلیلە نوێیەکانی گرووپ ===
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => new { gm.GroupId, gm.UserId });

            modelBuilder.Entity<GroupFollower>()
                .HasKey(gf => new { gf.GroupId, gf.UserId });
            modelBuilder.Entity<Group>().ToTable("AppGroups");
            modelBuilder.Entity<Group>().HasIndex(g => g.Username).IsUnique();

            // --- پێناسەکردنی پەیوەندییەکان (Relationships) ---

            // پەیوەندی Follow
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

            // پەیوەندی Notification
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

            // پەیوەندی Post و Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // پەیوەندی Post و Like
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- پێناسەکردنی تایبەتمەندییەکان (Properties) ---

            // بۆ سپۆنسەر
            modelBuilder.Entity<AdCampaign>()
                .OwnsOne(c => c.Audience);

            // === زیادکرا: دڵنیابوونەوە لەوەی ناوی بەکارهێنەری گرووپ تاکە ===
            modelBuilder.Entity<Group>()
                .HasIndex(g => g.Username)
                .IsUnique();

        }
    }
}
