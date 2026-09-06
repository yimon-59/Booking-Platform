using Microsoft.AspNetCore.Mvc;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Interfaces;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("bookings")]
    public async Task<ActionResult<BookingDto>> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _bookingService.CreateBookingAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("bookings/cancel")]
    public async Task<ActionResult<BookingDto>> CancelBooking(
        [FromQuery] int bookingId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _bookingService.CancelBookingAsync(
                bookingId,
                cancellationToken);

            return Ok(new
            {
                message = "Booking cancelled successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}