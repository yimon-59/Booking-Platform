using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezerv.Application.DTOs.Bookings
{
    public class JoinWaitlistRequest
    {
        public int UserId { get; set; }
        public int TimetableScheduleId { get; set; }
    }
}
