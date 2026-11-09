using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext(DbContextOptions<RaceDayContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<Result> Results => Set<Result>();

    // These mappings deliberately match the original Part 1 SQL script.
    // In particular, EventEnrolment.partcipantsID is misspelled in that
    // database script, so EF must map ParticipantsID to that exact column.
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleID);
            e.Property(x => x.RoleName).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserID);
            e.Property(x => x.FirstName).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.Phone).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime");
            e.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleID);
        });

        b.Entity<Event>(e =>
        {
            e.ToTable("Events");
            e.HasKey(x => x.EventID);
            e.Property(x => x.EventName).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.EventDescription).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.EventDate).HasColumnType("date");
            e.Property(x => x.EventLocation).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.EventStatus).HasColumnType("varchar(50)").HasMaxLength(50).HasDefaultValue("open").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.HasOne(x => x.Organiser).WithMany(x => x.Events)
                .HasForeignKey(x => x.OrganiserID).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.CategoryID);
            e.Property(x => x.CategoryName).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            e.Property(x => x.MaxParticipants).IsRequired();
            e.Property(x => x.EntryFee).HasColumnType("decimal(10,2)").IsRequired();
            e.HasOne(x => x.Event).WithMany(x => x.Categories)
                .HasForeignKey(x => x.EventID).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<EventEnrolment>(e =>
        {
            e.ToTable("EventEnrolment");
            e.HasKey(x => x.EnrolmentID);
            e.Property(x => x.ParticipantsID).HasColumnName("partcipantsID");
            e.Property(x => x.EnrolmentDate).HasColumnType("datetime2");
            e.Property(x => x.Status).HasColumnType("varchar(20)").HasMaxLength(20).HasDefaultValue("Active").IsRequired();
            e.HasIndex(x => new { x.ParticipantsID, x.EventID }).IsUnique();
            e.HasOne(x => x.Event).WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.EventID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Participant).WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.ParticipantsID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.CategoryID).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Result>(e =>
        {
            e.ToTable("Results");
            e.HasKey(x => x.ResultID);
            e.Property(x => x.FinishTime).HasColumnType("time");
            e.Property(x => x.Score).HasColumnType("decimal(10,2)");
            e.Property(x => x.RecordedAt).HasColumnType("datetime2");
            e.HasIndex(x => x.EnrolmentID).IsUnique();
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result)
                .HasForeignKey<Result>(x => x.EnrolmentID).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
