namespace Rezerv.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserPackage> UserPackages { get; set; }  = new List<UserPackage>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
}