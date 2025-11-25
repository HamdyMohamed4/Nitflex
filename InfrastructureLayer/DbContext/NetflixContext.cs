using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer
{
    public class NetflixContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public NetflixContext(DbContextOptions<NetflixContext> options)
            : base(options) { }

        // --- Core Content ---
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<TVShow> TVShows { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<CastMember> CastMembers { get; set; }

        // --- User-related tables ---
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserProfile> Profiles { get; set; }

        // --- Profile-based user activity ---
        public DbSet<UserWatchlist> UserWatchlists { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }
        public DbSet<UserHistory> UserHistories { get; set; }

        // --- Identity & Security ---
        public DbSet<TbRefreshTokens> TbRefreshTokens { get; set; }
        public DbSet<TbPaymentMethod> TbPaymentMethod { get; set; }
        public DbSet<TbPaymentTransaction> TbPaymentTransaction { get; set; }
        public DbSet<EmailOtp> EmailOtp { get; set; }

        // --- Junction Tables ---
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<TVShowGenre> TVShowGenres { get; set; }
        public DbSet<MovieCast> MovieCasts { get; set; }
        public DbSet<TvShowCast> TVShowCasts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        // inside NetflixContext class
        public DbSet<TransferRequest> TransferRequests { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Identity Tables ---
            modelBuilder.HasDefaultSchema("Identity");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // --- Composite Keys for Junction Tables ---
            modelBuilder.Entity<MovieGenre>().HasKey(mg => new { mg.MovieId, mg.GenreId });
            modelBuilder.Entity<TVShowGenre>().HasKey(tg => new { tg.TVShowId, tg.GenreId });
            modelBuilder.Entity<MovieCast>().HasKey(mc => new { mc.MovieId, mc.CastMemberId });
            modelBuilder.Entity<TvShowCast>().HasKey(tc => new { tc.TvShowId, tc.CastMemberId });

            // --- Profile-based Keys & Indexes ---
            modelBuilder.Entity<UserWatchlist>()
                .HasIndex(uw => new { uw.ProfileId, uw.ContentId, uw.ContentType })
                .IsUnique();

            modelBuilder.Entity<UserRating>()
                .HasIndex(ur => new { ur.ProfileId, ur.ContentId, ur.ContentType })
                .IsUnique();

            modelBuilder.Entity<UserHistory>()
                .HasIndex(uh => new { uh.ProfileId, uh.ContentId, uh.ContentType })
                .IsUnique();

            // --- Relationships ---
            modelBuilder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithMany(u => u.Profiles)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWatchlist>()
                .HasOne(uw => uw.Profile)
                .WithMany(p => p.WatchlistItems)
                .HasForeignKey(uw => uw.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.Profile)
                .WithMany(p => p.Ratings)
                .HasForeignKey(ur => ur.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserHistory>()
                .HasOne(uh => uh.Profile)
                .WithMany(p => p.Histories)
                .HasForeignKey(uh => uh.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Indexing for Content ---
            modelBuilder.Entity<Movie>().HasIndex(m => m.Title);
            modelBuilder.Entity<TVShow>().HasIndex(t => t.Title);
        }
    }
}
