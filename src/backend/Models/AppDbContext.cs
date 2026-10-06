/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Models;

/// <exclude />
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, ApplicationRole, int>(options)
{
    public DbSet<Setting> Settings { get; set; }
    public override DbSet<User> Users { get; set; }
    public DbSet<UserStreak> UserStreaks { get; set; }
    public DbSet<UserTour> UserTours { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemUrn> ItemUrns { get; set; }
    public DbSet<ScopeItem> ScopeItems { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<ScopeMembership> ScopeMemberships { get; set; }
    public DbSet<ScopeEdge> ScopeEdges { get; set; }
    public DbSet<UserScopeProgress> UserScopeProgress { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<ItemAnswer> Answers { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<LevelResult> LevelResults { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<UserCurrency> UserCurrencies { get; set; }
    public DbSet<Cosmetic> Cosmetics { get; set; }
    public DbSet<UserCosmetic> UserCosmetics { get; set; }

    public DbSet<Badge> Badges { get; set; }
    public DbSet<BadgeState> BadgeStates { get; set; }
    public DbSet<BadgeTriggerCondition> BadgeTriggerConditions { get; set; }
    public DbSet<ParameterizedBadgeEntry> ParameterizedBadgeEntries { get; set; }
    public DbSet<BadgeProgress> BadgeProgresses { get; set; }
    public DbSet<BadgeStamp> BadgeStamps { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<UserCharacter> UserCharacters { get; set; }

    //
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder
    //         .UseValidationCheckConstraints(options => options.UseRegex(false));
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Please avoid adding constraints here as much as possible. Try to specify those in the models via attributes instead.

        // Generates report datetime
        modelBuilder.Entity<Report>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<LevelResult>()
            .Property(l => l.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<UserAnswer>()
            .Property(l => l.AnswerDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // cannot be configured via attributes
        modelBuilder.Entity<Item>()
            .HasMany(i => i.Scopes)
            .WithMany(d => d.Items)
            .UsingEntity<ScopeItem>();

        DateTime seedTime = new(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Setting>().HasData([
            new Setting { Id = 1, LastActive = seedTime, ProfileName = "Profile 1"},
            new Setting { Id = 2, LastActive = seedTime, ProfileName = "Profile 2"},
        ]);
        modelBuilder.Entity<Currency>().HasData(
        [
            new Currency { Id = 1, Name = "NoseCoin", Sprite = "/customisation/bill.svg", StartingAmount = 100 }
        ]);

        modelBuilder.Entity<ScopeEdge>(builder =>
        {
            builder.HasKey(se => new { se.FromScopeId, se.ToScopeId });

            builder.HasOne(se => se.FromScope)
                .WithMany()
                .HasForeignKey(se => se.FromScopeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(se => se.ToScope)
                .WithMany()
                .HasForeignKey(se => se.ToScopeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_ScopeDependency_Weight_Positive",
                    "\"Weight\" >= 0 AND \"Weight\" <= 1"
                );
            });
        });

        modelBuilder.Entity<ScopeMembership>(entity =>
        {
            entity.HasKey(sm => new { sm.AncestorId, sm.DescendantId });

            entity.HasOne(sm => sm.Ancestor)
                .WithMany(n => n.Ancestors)
                .HasForeignKey(sm => sm.AncestorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sm => sm.Descendant)
                .WithMany(n => n.Descendants)
                .HasForeignKey(sm => sm.DescendantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserScopeProgress>().HasKey(up => new { up.UserId, up.ScopeId });
        modelBuilder.Entity<ItemUrn>().HasKey(itemUrn => itemUrn.ItemId);
        modelBuilder.Entity<UserStreak>().HasKey(userStreak => userStreak.UserId);
        modelBuilder.Entity<UserTour>().HasKey(t => new { t.UserId, t.TourKey });

        modelBuilder.Entity<UserCharacter>(entity =>
        {
            // Ensure a user can only have one character selected at once
            entity.HasIndex(uc => uc.UserId)
             .HasFilter("\"Selected\" = true")
             .IsUnique();

            entity.HasKey(userCharacter => new { userCharacter.UserId, userCharacter.CharacterId });
        });

        modelBuilder.Entity<ParameterizedBadgeEntry>()
            .Property(e => e.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!
            );

        modelBuilder.Entity<ParameterizedBadgeEntry>()
            .HasOne(p => p.Topic)
            .WithMany()
            .HasForeignKey(p => p.ScopeId)
            .OnDelete(DeleteBehavior.Cascade);

        
        modelBuilder.Entity<ParameterizedBadgeEntry>()
            .ToTable(t => t.HasCheckConstraint("CK_Amount_GreaterThanZero", "\"Amount\" > 0"));
    }
}
