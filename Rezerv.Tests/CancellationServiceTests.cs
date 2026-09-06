using Microsoft.EntityFrameworkCore;
using Rezerv.Application.Services;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;
using Xunit;

namespace Rezerv.Tests;

public class CancellationServiceTests
{
    [Fact]
    public async Task CancelBookingAsync_Should_Refund_Credit_When_Cancelled_More_Than_Four_Hours_Before_Start()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 4);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            startTime: DateTime.UtcNow.AddHours(6));

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id,
            Status = BookingStatus.Confirmed,
            BookedAt = DateTime.UtcNow
        };

        dbContext.Bookings.Add(booking);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        await service.CancelBookingAsync(booking.Id);

        var cancelledBooking = await dbContext.Bookings
            .FirstAsync(x => x.Id == booking.Id);

        var savedPackage = await dbContext.UserPackages
            .FirstAsync(x => x.Id == userPackage.Id);

        Assert.Equal(
            BookingStatus.Cancelled,
            cancelledBooking.Status);

        Assert.NotNull(cancelledBooking.CancelledAt);

        Assert.Equal(
            5,
            savedPackage.RemainingCredits);
    }


    [Fact]
    public async Task CancelBookingAsync_Should_Not_Refund_When_Cancelled_Within_Four_Hours()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 4);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            startTime: DateTime.UtcNow.AddHours(2));

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id,
            Status = BookingStatus.Confirmed,
            BookedAt = DateTime.UtcNow
        };

        dbContext.Bookings.Add(booking);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        await service.CancelBookingAsync(booking.Id);

        var cancelledBooking = await dbContext.Bookings
            .FirstAsync(x => x.Id == booking.Id);

        var savedPackage = await dbContext.UserPackages
            .FirstAsync(x => x.Id == userPackage.Id);

        Assert.Equal(
            BookingStatus.Cancelled,
            cancelledBooking.Status);

        Assert.Equal(
            4,
            savedPackage.RemainingCredits);
    }


    [Fact]
    public async Task CancelBookingAsync_Should_Reject_Already_Cancelled_Booking()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 4);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            startTime: DateTime.UtcNow.AddHours(6));

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id,
            Status = BookingStatus.Cancelled,
            BookedAt = DateTime.UtcNow,
            CancelledAt = DateTime.UtcNow
        };

        dbContext.Bookings.Add(booking);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CancelBookingAsync(booking.Id));

        Assert.Equal(
            "Booking has already been cancelled.",
            exception.Message);
    }


    [Fact]
    public async Task CancelBookingAsync_Should_Reject_When_Booking_Does_Not_Exist()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CancelBookingAsync(999999));

        Assert.Equal(
            "Booking not found.",
            exception.Message);
    }
}