using Rezerv.Application.DTOs.Bookings;

namespace Rezerv.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default);

    Task CancelBookingAsync(
        int bookingId,
        CancellationToken cancellationToken = default);

}