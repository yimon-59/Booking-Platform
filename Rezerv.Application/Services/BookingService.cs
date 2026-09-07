using Microsoft.EntityFrameworkCore;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Interfaces;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Application.Services;

public class BookingService : IBookingService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDistributedLockService _distributedLockService;

    public BookingService(IApplicationDbContext dbContext, IDistributedLockService distributedLockService)
    {
        _dbContext = dbContext;
        _distributedLockService = distributedLockService;
    }

    public async Task<BookingDto> CreateBookingAsync( CreateBookingRequest request,
      CancellationToken cancellationToken = default)
    {
        var lockKey = $"booking:schedule:{request.TimetableScheduleId}";

        await using var distributedLock =
            await _distributedLockService.AcquireAsync(
                lockKey,
                TimeSpan.FromSeconds(30),
                cancellationToken);

        if (distributedLock is null)
            throw new InvalidOperationException(
                "Another booking is currently being processed for this schedule. Please try again.");

        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var schedule =
                await _dbContext.GetTimetableScheduleForUpdateAsync(
                    request.TimetableScheduleId,
                    cancellationToken);

            if (schedule is null)
                throw new InvalidOperationException(
                    "Timetable schedule not found.");

            var user =
                await _dbContext.GetUserForUpdateAsync(
                    request.UserId,
                    cancellationToken);

            if (user is null)
                throw new InvalidOperationException(
                    "User not found.");

            var userPackage =
                await _dbContext.GetUserPackageForUpdateAsync(
                    request.UserPackageId,
                    cancellationToken);

            if (userPackage is null)
                throw new InvalidOperationException(
                    "Package not found.");

            if (userPackage.UserId != request.UserId || 
                userPackage.Id != request.UserPackageId) 
                throw new InvalidOperationException(
                    "User did not purchase this package.");

            await _dbContext.UserPackages
                .Where(x => x.Id == userPackage.Id)
                .Include(x => x.Package)
                .LoadAsync(cancellationToken);

            var now = DateTime.UtcNow;

            if (userPackage.Package.ExpiryDate <= now)
                throw new InvalidOperationException(
                    "Package has expired.");

            if (userPackage.RemainingCredits < 1)
                throw new InvalidOperationException(
                    "Insufficient credits.");

            if (userPackage.Package.BusinessId != schedule.BusinessId)
                throw new InvalidOperationException(
                    "Package does not belong to the same business as the schedule.");

            if (schedule.StartTime <= now)
                throw new InvalidOperationException(
                    "Cannot book a schedule that has already started.");

            var hasOverlappingBooking =
                await _dbContext.Bookings
                    .AnyAsync(
                        x =>
                            x.UserId == request.UserId &&
                            x.Status == BookingStatus.Confirmed &&
                            x.TimetableSchedule.StartTime < schedule.EndTime &&
                            x.TimetableSchedule.EndTime > schedule.StartTime,
                        cancellationToken);

            if (hasOverlappingBooking)
                throw new InvalidOperationException(
                    "User already has an overlapping booking.");

            var confirmedBookings =
                await _dbContext.Bookings
                    .CountAsync(
                        x =>
                            x.TimetableScheduleId == schedule.Id &&
                            x.Status == BookingStatus.Confirmed,
                        cancellationToken);

            if (confirmedBookings >= schedule.AvailableSlots)
                throw new InvalidOperationException(
                    "Schedule is full.");

            userPackage.RemainingCredits--;

            var booking = new Booking
            {
                UserId = request.UserId,
                UserPackageId = request.UserPackageId,
                TimetableScheduleId = schedule.Id,
                Status = BookingStatus.Confirmed,
                BookedAt = now
            };

            _dbContext.Bookings.Add(booking);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new BookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                UserPackageId = booking.UserPackageId,
                TimetableScheduleId = booking.TimetableScheduleId,
                ClassName = schedule.ClassName,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Status = booking.Status.ToString(),
                BookedAt =  TimeZoneInfo.ConvertTimeFromUtc(
                            booking.BookedAt,
                            TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time"))
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        var bookingInfo = await _dbContext.Bookings
            .Where(x => x.Id == bookingId)
            .Select(x => new
            {
                x.Id,
                x.TimetableScheduleId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (bookingInfo is null)
            throw new InvalidOperationException(
                "Booking not found.");

        var lockKey = $"booking:schedule:{bookingInfo.TimetableScheduleId}";

        await using var distributedLock = await _distributedLockService.AcquireAsync(
                                            lockKey,
                                            TimeSpan.FromSeconds(30),
                                            cancellationToken);

        if (distributedLock is null)
            throw new InvalidOperationException(
                "Another booking operation is currently being processed for this schedule. Please try again.");

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var booking = await _dbContext.Bookings
                .Include(x => x.TimetableSchedule)
                .Include(x => x.UserPackage)
                .FirstOrDefaultAsync(
                    x => x.Id == bookingId,
                    cancellationToken);

            if (booking is null)
                throw new InvalidOperationException(
                    "Booking not found.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException(
                    "Booking has already been cancelled.");

            var now = DateTime.UtcNow;

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = now;

            var canRefund = booking.TimetableSchedule.StartTime > now.AddHours(4);

            if (canRefund)
                booking.UserPackage.RemainingCredits++;

            var waitlistEntries = await _dbContext.WaitlistEntries
                .Where(x =>
                    x.TimetableScheduleId ==
                        booking.TimetableScheduleId &&
                    x.Status == WaitlistStatus.Waiting)
                .OrderBy(x => x.JoinedAt)
                .ToListAsync(cancellationToken);

            foreach (var waitlistEntry in waitlistEntries)
            {
                var userPackage =
                    await _dbContext.UserPackages
                        .Include(x => x.Package)
                        .Where(x =>
                            x.UserId == waitlistEntry.UserId &&
                            x.RemainingCredits >= 1 &&
                            x.Package.BusinessId == booking.TimetableSchedule.BusinessId &&
                            x.Package.ExpiryDate > now)
                        .OrderBy(x => x.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                if (userPackage is null)
                {
                    continue;
                }

                userPackage.RemainingCredits--;

                var promotedBooking = new Booking
                {
                    UserId = waitlistEntry.UserId,
                    UserPackageId = userPackage.Id,
                    TimetableScheduleId = booking.TimetableScheduleId,
                    Status = BookingStatus.Confirmed,
                    BookedAt = now
                };

                _dbContext.Bookings.Add(promotedBooking);

                waitlistEntry.Status = WaitlistStatus.Promoted;
                waitlistEntry.PromotedAt = now;
                break;
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);
            throw;
        }
    }
}