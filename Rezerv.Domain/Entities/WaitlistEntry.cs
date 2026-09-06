using Rezerv.Domain.Enums;

namespace Rezerv.Domain.Entities;

public class WaitlistEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TimetableScheduleId { get; set; }

    public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PromotedAt { get; set; }

    public User User { get; set; } = null!;

    public TimetableSchedule TimetableSchedule { get; set; } = null!;
}