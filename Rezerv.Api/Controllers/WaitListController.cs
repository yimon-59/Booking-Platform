using Microsoft.AspNetCore.Mvc;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Interfaces;

namespace Rezerv.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class WaitListController : ControllerBase
    {
        IWaitListService _waitListService;
        public WaitListController(IWaitListService waitListService)
        {
            _waitListService = waitListService;
        }

        [HttpPost("waitlist")]
        public async Task<ActionResult<BookingDto>> JoinWaitlist(
       [FromBody] JoinWaitlistRequest request,
       CancellationToken cancellationToken)
        {
            try
            {
                await _waitListService.JoinWaitlistAsync(
                    request,
                    cancellationToken);

                return Ok();
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
}
