using Microsoft.EntityFrameworkCore;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Interfaces;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Application.Services
{
    public class WaitListService : IWaitListService
    {
        private readonly IApplicationDbContext _dbContext;

        public WaitListService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task JoinWaitlistAsync(JoinWaitlistRequest waitlistRequest, CancellationToken cancellationToken = default)
        {
            var schedule = await _dbContext.TimetableSchedules
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == waitlistRequest.TimetableScheduleId, cancellationToken);

            if (schedule == null) throw new InvalidOperationException(
                       "Schedule not found.");

            int activeBookings = await _dbContext.Bookings
                .CountAsync(b => b.TimetableScheduleId == waitlistRequest.TimetableScheduleId && b.Status == BookingStatus.Confirmed
                , cancellationToken);

            if (activeBookings < schedule.AvailableSlots)
                throw new InvalidOperationException(
                       "User should book directly instead.");

            bool alreadyOnWaitlist = await _dbContext.WaitlistEntries
            .AnyAsync(w => w.TimetableScheduleId == waitlistRequest.TimetableScheduleId &&
            w.UserId == waitlistRequest.UserId && w.Status == WaitlistStatus.Waiting,
            cancellationToken);

            if (alreadyOnWaitlist) throw new InvalidOperationException(
                       "Already on waitlist.");

            bool hasValidPackage = await _dbContext.UserPackages
             .AnyAsync(up => up.UserId == waitlistRequest.UserId
                          && up.Package.BusinessId == schedule.BusinessId
                          && up.Package.ExpiryDate > DateTime.UtcNow
                          && up.RemainingCredits >= 1,
                          cancellationToken);
            if (!hasValidPackage) throw new InvalidOperationException(
                       "Package is not valid.");

            var waitlistEntry = new WaitlistEntry
            {
                UserId = waitlistRequest.UserId,
                TimetableScheduleId = waitlistRequest.TimetableScheduleId,
                Status = WaitlistStatus.Waiting,
                JoinedAt = DateTime.UtcNow
            };

            _dbContext.WaitlistEntries.Add(waitlistEntry);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ExpireWaitlistEntriesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var expiredEntries = await _dbContext.WaitlistEntries
                .Where(x =>
                    x.Status == WaitlistStatus.Waiting &&
                    x.TimetableSchedule.EndTime <= now)
                .ToListAsync(cancellationToken);

            foreach (var entry in expiredEntries)
            {
                entry.Status = WaitlistStatus.Expired;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
