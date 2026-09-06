using Microsoft.AspNetCore.Mvc;
using Rezerv.Application.Interfaces;
using System.Globalization;

namespace Rezerv.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TimetableScheduleController : ControllerBase
    {
        private readonly ITimetableService _timetableService;
        public TimetableScheduleController(ITimetableService timetableService) 
        { 
           _timetableService = timetableService;
        }

        [HttpGet("timetable")]
        public async Task<IActionResult> GetTimetable([FromQuery] int? businessId,[FromQuery] string? date)
        {
            DateOnly? parsedDate = null;

            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateOnly.TryParseExact(
                        date,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dateResult))
                {
                    return BadRequest(new
                    {
                        message = "Invalid date format. Expected format: yyyy-MM-dd"
                    });
                }

                parsedDate = dateResult;
            }

            var result = await _timetableService.GetTimetableListAsync(
                businessId,
                parsedDate);

            return Ok(result);
        }
    }
}
