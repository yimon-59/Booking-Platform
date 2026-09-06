namespace Rezerv.Application.DTOs.Bookings;

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int UserPackageId { get; set; }
    public int TimetableScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
}