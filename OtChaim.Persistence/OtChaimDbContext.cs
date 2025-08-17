using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtChaim.Domain.Common;
using OtChaim.Domain.EmergencyEvents;
using OtChaim.Domain.Users;

namespace OtChaim.Persistence;

/// <summary>
/// Entity Framework Core database context for the OtChaim application.
/// </summary>
public class OtChaimDbContext(DbContextOptions<OtChaimDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the emergencies database set.
    /// </summary>
    public DbSet<Emergency> Emergencies { get; set; }
    /// <summary>
    /// Gets or sets the users database set.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configures the database model.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Emergency configuration
        modelBuilder.Entity<Emergency>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(e => e.Location, ConfigureLocation);
            builder.OwnsMany(e => e.AffectedAreas, area =>
            {
                area.WithOwner();
                area.OwnsOne(a => a.Center, ConfigureLocation);
                area.Property(a => a.RadiusInMeters);
            });
            builder.OwnsMany(e => e.Responses, resp =>
            {
                resp.Property(r => r.UserId);
                resp.Property(r => r.IsSafe);
                resp.Property(r => r.Message);
                resp.Property(r => r.RespondedAt);
            });
            builder.OwnsOne(e => e.Attachments, attachments =>
            {
                attachments.Property(a => a.IncludePersonalInfo);
                attachments.Property(a => a.IncludeMedicalInfo);
                attachments.Property(a => a.IncludeGpsLocation);
                attachments.Property(a => a.PicturePath);
                attachments.Property(a => a.DocumentPath);
                attachments.Property(a => a.SendEmail);
                attachments.Property(a => a.SendSms);
                attachments.Property(a => a.SendMessenger);
            });
        });

        // User configuration
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Name);
            builder.Property(u => u.Email);
            builder.Property(u => u.PhoneNumber);
            builder.Property(u => u.Birthday);
            builder.Property(u => u.WeightKg);
            builder.Property(u => u.BloodType);
            builder.Property(u => u.Address);
            builder.OwnsOne(u => u.CurrentLocation, ConfigureLocation);
            builder.Property(u => u.ProfilePicturePath);
            builder.OwnsOne(u => u.PersonName, pn =>
            {
                pn.Property(p => p.First);
                pn.Property(p => p.Last);
            });
            builder.OwnsMany(u => u.NotificationChannels, nc =>
            {
                nc.Property(n => n.ChannelType);
                nc.Property(n => n.Address);
            });
            builder.OwnsMany(u => u.Subscriptions, sub =>
            {
                sub.Property(s => s.SubscriberId);
                sub.Property(s => s.SubscribedToId);
                sub.Property(s => s.Status);
                sub.Property(s => s.CreatedAt);
                sub.Property(s => s.ApprovedAt);
            });
        });

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Configures the location value object.
    /// </summary>
    private static void ConfigureLocation<T>(OwnedNavigationBuilder<T, Location> loc) where T : class
    {
        loc.WithOwner();
        loc.Property(l => l.Latitude);
        loc.Property(l => l.Longitude);
        loc.Property(l => l.Description);
    }
}
