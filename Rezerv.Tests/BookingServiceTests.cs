using Microsoft.EntityFrameworkCore;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Services;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateBookingAsync_Should_Create_Booking_And_Deduct_One_Credit()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(
            business,
            credits: 10);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 5);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            availableSlots: 5);

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        var result = await service.CreateBookingAsync(request);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(userPackage.Id, result.UserPackageId);
        Assert.Equal(schedule.Id, result.TimetableScheduleId);
        Assert.Equal("Confirmed", result.Status);

        var savedPackage = await dbContext.UserPackages
            .FirstAsync(x => x.Id == userPackage.Id);

        Assert.Equal(4, savedPackage.RemainingCredits);

        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id &&
                x.TimetableScheduleId == schedule.Id);

        Assert.NotNull(booking);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_When_Credits_Are_Insufficient()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 0);

        var schedule = TestDataFactory.CreateSchedule(business);

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        Assert.Equal(
            "Insufficient credits.",
            exception.Message);

        Assert.Equal(
            0,
            await dbContext.Bookings.CountAsync());
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_Expired_Package()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(
            business,
            credits: 10,
            expiryDate: DateTime.UtcNow.AddDays(-1));

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 5);

        var schedule = TestDataFactory.CreateSchedule(business);

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        Assert.Equal(
            "Package has expired.",
            exception.Message);

        Assert.Equal(
            0,
            await dbContext.Bookings.CountAsync());
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_When_Business_Does_Not_Match()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var packageBusiness = TestDataFactory.CreateBusiness(
            "Business A");

        var scheduleBusiness = TestDataFactory.CreateBusiness(
            "Business B");

        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(
            packageBusiness);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 5);

        var schedule = TestDataFactory.CreateSchedule(
            scheduleBusiness);

        dbContext.Businesses.Add(packageBusiness);
        dbContext.Businesses.Add(scheduleBusiness);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        Assert.Equal(
            "Package does not belong to the same business as the schedule.",
            exception.Message);
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_When_Schedule_Has_Already_Started()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 5);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            startTime: DateTime.UtcNow.AddMinutes(-30));

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        Assert.Equal(
            "Cannot book a schedule that has already started.",
            exception.Message);
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_Overlapping_Booking()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();
        var user = TestDataFactory.CreateUser();

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 5);

        var firstSchedule = TestDataFactory.CreateSchedule(
            business,
            startTime: DateTime.UtcNow.AddHours(2));

        var overlappingSchedule = TestDataFactory.CreateSchedule(
            business,
            startTime: DateTime.UtcNow.AddHours(2.5));

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(firstSchedule);
        dbContext.TimetableSchedules.Add(overlappingSchedule);

        await dbContext.SaveChangesAsync();

        var existingBooking = new Booking
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = firstSchedule.Id,
            Status = BookingStatus.Confirmed,
            BookedAt = DateTime.UtcNow
        };

        dbContext.Bookings.Add(existingBooking);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = overlappingSchedule.Id
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        Assert.Equal(
            "User already has an overlapping booking.",
            exception.Message);
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_When_Schedule_Is_Full()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        // Arrange
        var business = TestDataFactory.CreateBusiness();

        var package = TestDataFactory.CreatePackage(
            business,
            credits: 10);

        var user = TestDataFactory.CreateUser("Test User");

        var userPackage = TestDataFactory.CreateUserPackage(
            user,
            package,
            credits: 5);

        var schedule = TestDataFactory.CreateSchedule(
            business,
            availableSlots: 1);

        dbContext.Businesses.Add(business);
        dbContext.Packages.Add(package);
        dbContext.Users.Add(user);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        // Create another user who already occupies the only slot.
        var otherUser = TestDataFactory.CreateUser("Other User");

        var otherUserPackage = TestDataFactory.CreateUserPackage(
            otherUser,
            package,
            credits: 5);

        dbContext.Users.Add(otherUser);

        await dbContext.SaveChangesAsync();

        dbContext.UserPackages.Add(otherUserPackage);

        await dbContext.SaveChangesAsync();

        // Existing confirmed booking.
        var existingBooking = new Booking
        {
            UserId = otherUser.Id,
            UserPackageId = otherUserPackage.Id,
            TimetableScheduleId = schedule.Id,
            Status = BookingStatus.Confirmed,
            BookedAt = DateTime.UtcNow
        };

        dbContext.Bookings.Add(existingBooking);

        await dbContext.SaveChangesAsync();

        // Create service.
        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        // Assert
        Assert.Equal(
            "Schedule is full.",
            exception.Message);

        // The failed booking must not be created.
        var bookingCount = await dbContext.Bookings
            .CountAsync(x =>
                x.TimetableScheduleId == schedule.Id &&
                x.Status == BookingStatus.Confirmed);

        Assert.Equal(1, bookingCount);

        // The requesting user's credit must not be deducted.
        var savedPackage = await dbContext.UserPackages
            .FirstAsync(x => x.Id == userPackage.Id);

        Assert.Equal(5, savedPackage.RemainingCredits);
    }


    [Fact]
    public async Task CreateBookingAsync_Should_Reject_When_User_Does_Not_Own_Package()
    {
        await TestDbContextFactory.ResetDatabaseAsync();

        await using var dbContext = TestDbContextFactory.Create();

        var business = TestDataFactory.CreateBusiness();

        var user1 = TestDataFactory.CreateUser("User 1");
        var user2 = TestDataFactory.CreateUser("User 2");

        var package = TestDataFactory.CreatePackage(business);

        var userPackage = TestDataFactory.CreateUserPackage(
            user1,
            package,
            credits: 5);

        var schedule = TestDataFactory.CreateSchedule(business);

        dbContext.Businesses.Add(business);
        dbContext.Users.Add(user1);
        dbContext.Users.Add(user2);
        dbContext.Packages.Add(package);
        dbContext.UserPackages.Add(userPackage);
        dbContext.TimetableSchedules.Add(schedule);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(
            dbContext,
            new FakeDistributedLockService());

        var request = new CreateBookingRequest
        {
            UserId = user2.Id,
            UserPackageId = userPackage.Id,
            TimetableScheduleId = schedule.Id
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBookingAsync(request));

        Assert.Equal(
            "User did not purchase this package.",
            exception.Message);
    }
}