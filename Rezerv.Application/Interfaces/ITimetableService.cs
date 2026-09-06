using Rezerv.Application.DTOs.Packages;
using Rezerv.Application.DTOs.Timetable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezerv.Application.Interfaces
{
    public interface ITimetableService
    {
        Task<List<TimetableSchduleDto>> GetTimetableListAsync(
        int? businessId,
        DateOnly? date);
    }
}
