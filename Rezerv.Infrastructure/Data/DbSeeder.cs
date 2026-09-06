using Microsoft.EntityFrameworkCore;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RezervDbContext context)
    {
        // Don't seed again if data already exists
        if (await context.Users.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // =========================
        // BUSINESSES
        // =========================

        var business1 = new Business
        {
            Name = "FitLife Studio",
            CreatedAt = now
        };

        var business2 = new Business
        {
            Name = "PowerHouse Fitness",
            CreatedAt = now
        };

        context.Businesses.AddRange(business1, business2);
        await context.SaveChangesAsync();


        // =========================
        // USERS
        // =========================

        var users = Enumerable.Range(1, 10)
            .Select(i => new User
            {
                Name = $"Test User {i}",
                Email = $"user{i}@example.com",
                CreatedAt = now.AddDays(-i)
            })
            .ToList();

        context.Users.AddRange(users);
        await context.SaveChangesAsync();


        // =========================
        // PACKAGES
        // =========================

        var activePackage1 = new Package
        {
            Name = "FitLife 10 Credits",
            BusinessId = business1.Id,
            TotalCredits = 10,
            ExpiryDate = now.AddDays(30)
        };

        var activePackage2 = new Package
        {
            Name = "FitLife 20 Credits",
            BusinessId = business1.Id,
            TotalCredits = 20,
            ExpiryDate = now.AddDays(60)
        };

        var activePackage3 = new Package
        {
            Name = "PowerHouse 10 Credits",
            BusinessId = business2.Id,
            TotalCredits = 10,
            ExpiryDate = now.AddDays(30)
        };

        var expiredPackage = new Package
        {
            Name = "Expired Package",
            BusinessId = business1.Id,
            TotalCredits = 10,
            ExpiryDate = now.AddDays(-10)
        };

        context.Packages.AddRange(
            activePackage1,
            activePackage2,
            activePackage3,
            expiredPackage);

        await context.SaveChangesAsync();


        // =========================
        // USER PACKAGES
        // =========================

        var userPackages = new List<UserPackage>
        {
            new()
            {
                UserId = users[0].Id,
                PackageId = activePackage1.Id,
                RemainingCredits = 10,
                PurchasedAt = now.AddDays(-5)
            },

            new()
            {
                UserId = users[1].Id,
                PackageId = activePackage1.Id,
                RemainingCredits = 5,
                PurchasedAt = now.AddDays(-4)
            },

            new()
            {
                UserId = users[2].Id,
                PackageId = activePackage2.Id,
                RemainingCredits = 15,
                PurchasedAt = now.AddDays(-3)
            },

            new()
            {
                UserId = users[3].Id,
                PackageId = activePackage3.Id,
                RemainingCredits = 8,
                PurchasedAt = now.AddDays(-2)
            },

            new()
            {
                UserId = users[4].Id,
                PackageId = activePackage1.Id,
                RemainingCredits = 3,
                PurchasedAt = now.AddDays(-2)
            },

            new()
            {
                UserId = users[5].Id,
                PackageId = activePackage1.Id,
                RemainingCredits = 2,
                PurchasedAt = now.AddDays(-1)
            },

            new()
            {
                UserId = users[6].Id,
                PackageId = activePackage3.Id,
                RemainingCredits = 5,
                PurchasedAt = now
            },

            new()
            {
                UserId = users[7].Id,
                PackageId = activePackage2.Id,
                RemainingCredits = 10,
                PurchasedAt = now
            },

            new()
            {
                UserId = users[8].Id,
                PackageId = expiredPackage.Id,
                RemainingCredits = 5,
                PurchasedAt = now.AddDays(-20)
            }
        };

        context.UserPackages.AddRange(userPackages);
        await context.SaveChangesAsync();


        // =========================
        // TIMETABLE SCHEDULES
        // =========================

        var schedules = new List<TimetableSchedule>
        {
            // Schedule 1
            CreateSchedule(
                business1,
                "Morning Yoga",
                "Alice",
                now.AddDays(1).Date.AddHours(8),
                10),

            // Schedule 2
            CreateSchedule(
                business1,
                "Pilates",
                "Bob",
                now.AddDays(1).Date.AddHours(10),
                8),

            // Schedule 3
            CreateSchedule(
                business1,
                "HIIT",
                "Charlie",
                now.AddDays(1).Date.AddHours(14),
                5),

            // Schedule 4 - FULL
            CreateSchedule(
                business1,
                "Strength Training",
                "David",
                now.AddDays(1).Date.AddHours(16),
                2),

            // Schedule 5
            CreateSchedule(
                business1,
                "Evening Yoga",
                "Alice",
                now.AddDays(1).Date.AddHours(18),
                12),

            // Schedule 6
            CreateSchedule(
                business2,
                "CrossFit",
                "John",
                now.AddDays(2).Date.AddHours(8),
                10),

            // Schedule 7
            CreateSchedule(
                business2,
                "Boxing",
                "Mike",
                now.AddDays(2).Date.AddHours(10),
                6),

            // Schedule 8 - FULL
            CreateSchedule(
                business2,
                "Power Training",
                "Sarah",
                now.AddDays(2).Date.AddHours(14),
                3),

            // Schedule 9
            CreateSchedule(
                business2,
                "Cardio",
                "Emma",
                now.AddDays(2).Date.AddHours(16),
                15),

            // Schedule 10
            CreateSchedule(
                business2,
                "Functional Training",
                "James",
                now.AddDays(2).Date.AddHours(18),
                8)
        };

        context.TimetableSchedules.AddRange(schedules);
        await context.SaveChangesAsync();


        // =========================
        // EXISTING BOOKINGS
        // =========================

        // Fill Schedule 4 (2 available slots)
        var booking1 = new Booking
        {
            UserId = users[0].Id,
            UserPackageId = userPackages[0].Id,
            TimetableScheduleId = schedules[3].Id,
            Status = BookingStatus.Confirmed,
            BookedAt = now
        };

        var booking2 = new Booking
        {
            UserId = users[1].Id,
            UserPackageId = userPackages[1].Id,
            TimetableScheduleId = schedules[3].Id,
            Status = BookingStatus.Confirmed,
            BookedAt = now
        };

        // Fill Schedule 8 (3 available slots)
        var booking3 = new Booking
        {
            UserId = users[3].Id,
            UserPackageId = userPackages[3].Id,
            TimetableScheduleId = schedules[7].Id,
            Status = BookingStatus.Confirmed,
            BookedAt = now
        };

        var booking4 = new Booking
        {
            UserId = users[6].Id,
            UserPackageId = userPackages[6].Id,
            TimetableScheduleId = schedules[7].Id,
            Status = BookingStatus.Confirmed,
            BookedAt = now
        };

        var booking5 = new Booking
        {
            UserId = users[7].Id,
            UserPackageId = userPackages[7].Id,
            TimetableScheduleId = schedules[7].Id,
            Status = BookingStatus.Confirmed,
            BookedAt = now
        };

        context.Bookings.AddRange(
            booking1,
            booking2,
            booking3,
            booking4,
            booking5);

        await context.SaveChangesAsync();


        // =========================
        // WAITLIST SCENARIOS
        // =========================

        // Users 3 and 4 are waiting for Schedule 4.
        var waitlist1 = new WaitlistEntry
        {
            UserId = users[2].Id,
            TimetableScheduleId = schedules[3].Id,
            Status = WaitlistStatus.Waiting,
            JoinedAt = now.AddMinutes(-10)
        };

        var waitlist2 = new WaitlistEntry
        {
            UserId = users[4].Id,
            TimetableScheduleId = schedules[3].Id,
            Status = WaitlistStatus.Waiting,
            JoinedAt = now.AddMinutes(-5)
        };

        // Another waitlist scenario for Schedule 8
        var waitlist3 = new WaitlistEntry
        {
            UserId = users[5].Id,
            TimetableScheduleId = schedules[7].Id,
            Status = WaitlistStatus.Waiting,
            JoinedAt = now.AddMinutes(-2)
        };

        context.WaitlistEntries.AddRange(
            waitlist1,
            waitlist2,
            waitlist3);

        await context.SaveChangesAsync();
    }


    private static TimetableSchedule CreateSchedule(
        Business business,
        string className,
        string instructor,
        DateTime startTime,
        int availableSlots)
    {
        return new TimetableSchedule
        {
            BusinessId = business.Id,
            ClassName = className,
            Instructor = instructor,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            AvailableSlots = availableSlots
        };
    }
}