using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Package> Packages { get; }

    DbSet<UserPackage> UserPackages { get; }

    DbSet<TimetableSchedule> TimetableSchedules { get; }

    DbSet<Booking> Bookings { get; }

    DbSet<WaitlistEntry> WaitlistEntries { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task<TimetableSchedule?> GetTimetableScheduleForUpdateAsync(
        int scheduleId,
        CancellationToken cancellationToken = default);

    Task<User?> GetUserForUpdateAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserPackage?> GetUserPackageForUpdateAsync(
        int userPackageId,
        CancellationToken cancellationToken = default);
}