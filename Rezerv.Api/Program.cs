using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Rezerv.Api.BackgroundServices;
using Rezerv.Application.Interfaces;
using Rezerv.Application.Services;
using Rezerv.Infrastructure.Data;
using Rezerv.Infrastructure.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RezervDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString));
});
var redisConnection =
    builder.Configuration.GetConnectionString("Redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection!));

builder.Services.AddSingleton<IDistributedLockService,
    RedisDistributedLockService>();
// Add services to the container.
builder.Services.AddScoped<IApplicationDbContext, RezervDbContext>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IWaitListService, WaitListService>();
builder.Services.AddHostedService<WaitlistExpirationBackgroundService>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Booking System API",
        Version = "v1"
    });
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<RezervDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
