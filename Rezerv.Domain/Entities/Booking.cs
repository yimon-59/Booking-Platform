using Rezerv.Domain.Enums;

namespace Rezerv.Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int UserPackageId { get; set; }

    public int TimetableScheduleId { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CancelledAt { get; set; }

    public User User { get; set; } = null!;

    public UserPackage UserPackage { get; set; } = null!;

    public TimetableSchedule TimetableSchedule { get; set; } = null!;
}