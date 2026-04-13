using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Data;

public class BBScoreboardDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public BBScoreboardDbContext(DbContextOptions<BBScoreboardDbContext> options)
        : base(options) { }

    public DbSet<UCSeason> UCSeasons => Set<UCSeason>();
    public DbSet<UCTeam> UCTeams => Set<UCTeam>();
    public DbSet<UCPlayer> UCPlayers => Set<UCPlayer>();
    public DbSet<UCGame> UCGames => Set<UCGame>();
    public DbSet<UCGameTeamStat> UCGameTeamStats => Set<UCGameTeamStat>();
    public DbSet<UCGamePlayerStat> UCGamePlayerStats => Set<UCGamePlayerStat>();
    public DbSet<UCGameplayAction> UCGameplayActions => Set<UCGameplayAction>();
    public DbSet<BasketballPosition> BasketballPositions => Set<BasketballPosition>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // UCSeason
        builder.Entity<UCSeason>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(2000).IsRequired();
        });

        // UCTeam
        builder.Entity<UCTeam>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(2000).IsRequired();
            e.Property(x => x.TeamColor).HasMaxLength(50);
            e.Property(x => x.Active).HasDefaultValue(true);
        });

        // UCPlayer
        builder.Entity<UCPlayer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(500);
            e.Property(x => x.LastName).HasMaxLength(500);
            e.Property(x => x.Active).HasDefaultValue(true);
        });

        // UCGame
        builder.Entity<UCGame>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Venue).HasMaxLength(2000);
            e.Property(x => x.IsStarted).HasDefaultValue(false);
            e.Property(x => x.IsTimeOn).HasDefaultValue(false);
            e.Property(x => x.IsEnded).HasDefaultValue(false);
        });

        // UCGameTeamStat
        builder.Entity<UCGameTeamStat>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // UCGamePlayerStat
        builder.Entity<UCGamePlayerStat>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InFloor).HasDefaultValue(false);
        });

        // UCGameplayAction
        builder.Entity<UCGameplayAction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PlayerId).HasDefaultValue(-1);
            e.Property(x => x.RecPlayerId).HasDefaultValue(-1);
            e.Property(x => x.TeamScore1).HasDefaultValue(0);
            e.Property(x => x.TeamScore2).HasDefaultValue(0);
        });

        // BasketballPosition
        builder.Entity<BasketballPosition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(500).IsRequired();
            e.Property(x => x.ShortName).HasMaxLength(50).IsRequired();
        });

        // AppConfig
        builder.Entity<AppConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(500);
            e.Property(x => x.Value).HasMaxLength(500);
        });

        // Seed BasketballPositions
        builder.Entity<BasketballPosition>().HasData(
            new BasketballPosition { Id = 1, Name = "Point Guard", ShortName = "PG" },
            new BasketballPosition { Id = 2, Name = "Shooting Guard", ShortName = "SG" },
            new BasketballPosition { Id = 3, Name = "Small Forward", ShortName = "SF" },
            new BasketballPosition { Id = 4, Name = "Power Forward", ShortName = "PF" },
            new BasketballPosition { Id = 5, Name = "Center", ShortName = "C" }
        );
    }
}
