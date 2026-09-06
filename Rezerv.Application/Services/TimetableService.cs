using Microsoft.EntityFrameworkCore;
using Rezerv.Application.DTOs.Timetable;
using Rezerv.Application.Interfaces;
using Rezerv.Domain.Enums;

namespace Rezerv.Application.Services
{
    public class TimetableService : ITimetableService
    {
        IApplicationDbContext _context;

        public TimetableService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimetableSchduleDto>> GetTimetableListAsync(
        int? businessId,
        DateOnly? date)
        {
            var query = _context.TimetableSchedules
                .AsNoTracking()
                .AsQueryable();

            if (businessId.HasValue)
            {
                query = query.Where(x =>
                    x.BusinessId == businessId.Value);
            }

            if (date.HasValue)
            {
                var startDate = date.Value.ToDateTime(TimeOnly.MinValue);
                var endDate = startDate.AddDays(1);

                query = query.Where(x =>
                    x.StartTime >= startDate &&
                    x.StartTime < endDate);
            }

            return await query
                .Select(x => new TimetableSchduleDto
                {
                    SchduleId = x.Id,
                    ClassName = x.ClassName,
                    InstructorName = x.Instructor,
                    StartTime = x.StartTime.ToString(),
                    EndTime = x.EndTime.ToString(),

                    AttendanceCount = Convert.ToString(x.Bookings.Count(b => b.Status == BookingStatus.Confirmed)),


                    AvailableSlots = Convert.ToString(x.AvailableSlots),

                    BusinessName = x.Business.Name
                })
                .ToListAsync();
        }
    }
}
