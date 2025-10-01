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
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        [Obsolete]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //================================================================
            // == ناوی خشتەکان بۆ وشە پارێزراوەکان ==
            //================================================================
            modelBuilder.Entity<Group>().ToTable("AppGroups");


            //================================================================
            // == کلیلە تێکەڵەکان (Composite Keys) ==
            //================================================================
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => new { gm.GroupId, gm.UserId });

            modelBuilder.Entity<GroupFollower>()
                .HasKey(gf => new { gf.GroupId, gf.UserId });


            //================================================================
            // == پەیوەندییەکان (Relationships) ==
            //================================================================

            // --- پەیوەندی Follow ---
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

            // --- پەیوەندی Notification ---
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

            // --- پەیوەندییەکانی Like (نوێکراوەتەوە) ---
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.GroupPost)
                .WithMany() // GroupPost پەیوەندییەکی ڕاستەوخۆی نییە بۆ لایکەکان
                .HasForeignKey(l => l.GroupPostId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- پەیوەندییەکانی Comment (نوێکراوەتەوە) ---
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.GroupPost)
                .WithMany() // GroupPost پەیوەندییەکی ڕاستەوخۆی نییە بۆ کۆمێنتەکان
                .HasForeignKey(c => c.GroupPostId)
                .OnDelete(DeleteBehavior.Cascade);


            //================================================================
            // == یاسا و تایبەتمەندییەکان (Constraints & Properties) ==
            //================================================================

            // --- تایبەتمەندییەکان ---
            modelBuilder.Entity<AdCampaign>().OwnsOne(c => c.Audience);
            modelBuilder.Entity<Group>().HasIndex(g => g.Username).IsUnique();

            // --- یاساکانی Like و Comment (نوێ) ---
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.GroupPostId })
                .IsUnique();

          modelBuilder.Entity<Like>()
    .HasCheckConstraint("CK_Like_EntityType", "(`PostId` IS NOT NULL AND `GroupPostId` IS NULL) OR (`PostId` IS NULL AND `GroupPostId` IS NOT NULL)");

            modelBuilder.Entity<Comment>()
                .HasCheckConstraint("CK_Comment_EntityType", "(`PostId` IS NOT NULL AND `GroupPostId` IS NULL) OR (`PostId` IS NULL AND `GroupPostId` IS NOT NULL)");




            //bo Chating
            // دڵنیابوونەوە لەوەی گفتوگۆی دووبارە دروست نابێت
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => new { c.Participant1Id, c.Participant2Id })
                .IsUnique();

            // ڕێکخستنی پەیوەندی Forwarded Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ForwardedMessage)
                .WithMany()
                .HasForeignKey(m => m.ForwardedMessageId)
                .OnDelete(DeleteBehavior.SetNull); // ئەگەر نامە ئەسڵییەکە سڕایەوە، forwardـەکە نەسڕێتەوە
        }
    }
}
