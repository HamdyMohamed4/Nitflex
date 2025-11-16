using InfrastructureLayer.UserModels;
using Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
namespace InfrastructureLayer;

public class NetflixContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public NetflixContext(DbContextOptions<NetflixContext> options)
        : base(options) { }

    // Define all DbSet properties for custom tables
    // Note: Identity DbSets (Users, Roles, etc.) are inherited automatically.
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<TVShow> TVShows { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<CastMember> CastMembers { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    // DbSet for Junction/Relationship tables
    public DbSet<MovieGenre> MovieGenres { get; set; }
    public DbSet<TVShowGenre> TVShowGenres { get; set; }
    public DbSet<MovieCast> MovieCasts { get; set; }
    public DbSet<TvShowCast> TVShowCasts { get; set; }
    public DbSet<UserWatchlist> UserWatchlists { get; set; }
    public DbSet<UserRating> UserRatings { get; set; }
    public DbSet<TbRefreshTokens> TbRefreshTokens { get; set; }
    public DbSet<TbPaymentMethod> TbPaymentMethod { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must call base implementation to correctly configure Identity tables
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables for cleaner look (Optional but recommended)
        modelBuilder.HasDefaultSchema("Identity");
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        // ... other Identity table renames as needed

        // --- Define Composite Keys and Relationships for Junction Tables ---

        // MovieGenre: Composite Key and relationships (using Guid)
        modelBuilder.Entity<MovieGenre>().HasKey(mg => new { mg.MovieId, mg.GenreId });

        // TVShowGenre: Composite Key and relationships (using Guid)
        modelBuilder.Entity<TVShowGenre>().HasKey(tg => new { tg.TVShowId, tg.GenreId });

        // MovieCast: Composite Key and relationships (using Guid)
        modelBuilder.Entity<MovieCast>().HasKey(mc => new { mc.MovieId, mc.CastMemberId });

        // TvShowCast: Composite Key and relationships (using Guid)
        modelBuilder.Entity<TvShowCast>().HasKey(tc => new { tc.TvShowId, tc.CastMemberId });

        // UserWatchlist: Composite Index (using Guid for UserId and ContentId)
        modelBuilder.Entity<UserWatchlist>()
            .HasIndex(uw => new { uw.UserId, uw.ContentId, uw.ContentType })
            .IsUnique();

        // UserRating: Composite Index (using Guid for UserId and ContentId)
        modelBuilder.Entity<UserRating>()
            .HasIndex(ur => new { ur.UserId, ur.ContentId, ur.ContentType })
            .IsUnique();

        // --- Indexing for faster lookups ---
        modelBuilder.Entity<Movie>().HasIndex(m => m.Title);
        modelBuilder.Entity<TVShow>().HasIndex(t => t.Title);
    }
}