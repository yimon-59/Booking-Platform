using Rezerv.Domain.Entities;

namespace Rezerv.Tests;

public static class TestDataFactory
{
    public static Business CreateBusiness(
        string name = "Test Fitness")
    {
        return new Business
        {
            Name = name
        };
    }

    public static User CreateUser(
        string name = "Test User")
    {
        return new User
        {
            Name = name,
            Email = $"{Guid.NewGuid()}@test.local"
        };
    }

    public static Package CreatePackage(
        Business business,
        int credits = 10,
        DateTime? expiryDate = null)
    {
        return new Package
        {
            Name = "Test Package",
            Business = business,
            TotalCredits = credits,
            ExpiryDate = expiryDate ?? DateTime.UtcNow.AddDays(30)
        };
    }

    public static UserPackage CreateUserPackage(
        User user,
        Package package,
        int credits = 5)
    {
        return new UserPackage
        {
            User = user,
            Package = package,
            RemainingCredits = credits,
            PurchasedAt = DateTime.UtcNow
        };
    }

    public static TimetableSchedule CreateSchedule(
        Business business,
        int availableSlots = 5,
        DateTime? startTime = null)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(2);

        return new TimetableSchedule
        {
            Business = business,
            ClassName = "Yoga",
            Instructor = "John",
            StartTime = start,
            EndTime = start.AddHours(1),
            AvailableSlots = availableSlots
        };
    }
}