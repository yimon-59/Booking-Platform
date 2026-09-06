using Rezerv.Application.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezerv.Application.Interfaces
{
    public interface IWaitListService
    {
        Task JoinWaitlistAsync(JoinWaitlistRequest request,
            CancellationToken cancellationToken = default);

        Task ExpireWaitlistEntriesAsync(CancellationToken cancellationToken = default);
    }
}
