using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezerv.Application.DTOs.Timetable
{
    public class TimetableSchduleDto
    {
        public int SchduleId { get; set; }  
        public string ClassName {  get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string AttendanceCount { get; set; } = string.Empty;
        public string AvailableSlots { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
    }
}
