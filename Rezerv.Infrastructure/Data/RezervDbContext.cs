using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rezerv.Application.Interfaces;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Data;

public class RezervDbContext : DbContext, IApplicationDbContext
{
    public RezervDbContext(DbContextOptions<RezervDbContext> options)
        : base(options)
    {
    }
    public async Task<IDbContextTransaction> BeginTransactionAsync(
       CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(
            cancellationToken);
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<TimetableSchedule> TimetableSchedules => Set<TimetableSchedule>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();
    public DbSet<UserPackage> UserPackages => Set<UserPackage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureBusiness(modelBuilder);
        ConfigurePackage(modelBuilder);
        ConfigureUserPackage(modelBuilder);
        ConfigureTimetableSchedule(modelBuilder);
        ConfigureBooking(modelBuilder);
        ConfigureWaitlistEntry(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });
    }

    private static void ConfigureBusiness(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);
        });
    }

    private static void ConfigurePackage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
             .IsRequired();

            entity.Property(x => x.BusinessId)
              .IsRequired();

            entity.Property(x => x.TotalCredits)
              .IsRequired();

            entity.Property(x => x.ExpiryDate)
              .IsRequired();

            entity.HasOne(x => x.Business)
                .WithMany(x => x.Packages)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.BusinessId,
                x.ExpiryDate
            });
        });
    }

    private static void ConfigureUserPackage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPackage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
              .IsRequired();

            entity.Property(x => x.PackageId)
              .IsRequired();

            entity.Property(x => x.RemainingCredits)
              .IsRequired();

            entity.Property(x => x.PurchasedAt)
              .IsRequired();

            entity.HasOne(x => x.User)
              .WithMany(x => x.UserPackages)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Package)
                .WithMany(x => x.UserPackages)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTimetableSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimetableSchedule>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ClassName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Instructor)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasOne(x => x.Business)
                .WithMany(x => x.TimetableSchedules)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.BusinessId,
                x.StartTime
            });
        });
    }

    private static void ConfigureBooking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.UserPackageId)
                .IsRequired();

            entity.Property(x => x.TimetableScheduleId)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UserPackage)
                .WithMany()
                .HasForeignKey(x => x.UserPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TimetableSchedule)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.TimetableScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.TimetableScheduleId,
                x.Status
            });
        });
    }

    private static void ConfigureWaitlistEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WaitlistEntry>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.WaitlistEntries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TimetableSchedule)
                .WithMany(x => x.WaitlistEntries)
                .HasForeignKey(x => x.TimetableScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.TimetableScheduleId,
                x.Status,
                x.JoinedAt
            });
        });
    }

    public async Task<TimetableSchedule?> GetTimetableScheduleForUpdateAsync(int scheduleId, CancellationToken cancellationToken = default)
    {
        return await TimetableSchedules
        .FromSqlInterpolated($"""
            SELECT *
            FROM TimetableSchedules
            WHERE Id = {scheduleId}
            FOR UPDATE
            """)
        .AsTracking()
        .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetUserForUpdateAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Users
        .FromSqlInterpolated($"""
            SELECT *
            FROM Users
            WHERE Id = {userId}
            FOR UPDATE
            """)
        .AsTracking()
        .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserPackage?> GetUserPackageForUpdateAsync(int userPackageId, CancellationToken cancellationToken = default)
    {
        return await UserPackages
        .FromSqlInterpolated($"""
            SELECT *
            FROM UserPackages
            WHERE Id = {userPackageId}
            FOR UPDATE
            """)
        .AsTracking()
        .SingleOrDefaultAsync(cancellationToken);
    }
}