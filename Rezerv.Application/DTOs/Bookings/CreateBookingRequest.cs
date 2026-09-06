using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezerv.Application.DTOs.Bookings
{
    public class CreateBookingRequest
    {
        public int UserId { get; set; }
        public int UserPackageId { get; set; }
        public int TimetableScheduleId { get; set; }
    }
}
