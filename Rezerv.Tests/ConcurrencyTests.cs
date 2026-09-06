using Microsoft.EntityFrameworkCore;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Services;
using Rezerv.Domain.Enums;
using Xunit;

namespace Rezerv.Tests;

public class ConcurrencyTests
{
    [Fact]
    public async Task Concurrent_Bookings_Should_Not_Exceed_Available_Slots()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var setupContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();

        var package = TestDataFactory.CreatePackage(
            business,
            credits: 10);

        setupContext.Businesses.Add(business);
        setupContext.Packages.Add(package);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            availableSlots: 2);

        setupContext.TimetableSchedules.Add(schedule);

        var users = new List<(int UserId, int UserPackageId)>();

        for (var i = 1; i <= 5; i++)
        {
            var user = TestDataFactory.CreateUser(
                $"Concurrent User {i}");

            var userPackage = TestDataFactory.CreateUserPackage(
                user,
                package,
                credits: 1);

            setupContext.Users.Add(user);
            setupContext.UserPackages.Add(userPackage);

            await setupContext.SaveChangesAsync();

            users.Add((user.Id, userPackage.Id));
        }

        await setupContext.SaveChangesAsync();

        var tasks = users.Select(async item =>
        {
            await using var context = TestDbContextFactory.Create();

            var service = new BookingService(
                context,
                new FakeDistributedLockService());

            var request = new CreateBookingRequest
            {
                UserId = item.UserId,
                UserPackageId = item.UserPackageId,
                TimetableScheduleId = schedule.Id
            };

            try
            {
                await service.CreateBookingAsync(request);

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);

        var successfulBookings = results.Count(x => x);

        Assert.Equal(2, successfulBookings);

        await using var verificationContext =
            TestDbContextFactory.Create();

        var bookingCount = await verificationContext.Bookings
            .CountAsync(x =>
                x.TimetableScheduleId == schedule.Id &&
                x.Status == BookingStatus.Confirmed);

        Assert.Equal(2, bookingCount);

        var scheduleFromDatabase =
            await verificationContext.TimetableSchedules
                .FirstAsync(x => x.Id == schedule.Id);

        Assert.True(
            bookingCount <= scheduleFromDatabase.AvailableSlots);
    }
}