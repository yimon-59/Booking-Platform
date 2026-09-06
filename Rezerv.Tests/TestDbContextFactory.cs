using Microsoft.EntityFrameworkCore;
using Rezerv.Infrastructure.Data;

namespace Rezerv.Tests;

public static class TestDbContextFactory
{
    private const string ConnectionString =
        "Server=localhost;Port=3306;Database=RezervBooking_test;User=root;Password=111111;";

    public static RezervDbContext Create()
    {
        var options = new DbContextOptionsBuilder<RezervDbContext>()
            .UseMySql(
                ConnectionString,
                ServerVersion.AutoDetect(ConnectionString))
            .Options;

        return new RezervDbContext(options);
    }

    public static async Task ResetDatabaseAsync()
    {
        await using var context = Create();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}