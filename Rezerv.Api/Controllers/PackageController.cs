using Microsoft.AspNetCore.Mvc;
using Rezerv.Application.DTOs.Packages;
using Rezerv.Application.Interfaces;

namespace Rezerv.Api.Controllers
{

    [ApiController]
    [Route("api")]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _packageService.GetPackagesAsync();
            return Ok(packages);
        }

        [HttpGet("packages/userPackages")]
        public async Task<IActionResult> GetUserPackages([FromQuery] int userId)
        {
            var packages = await _packageService.GetUserPackagesAsync(userId);
            return Ok(packages);
        }

        [HttpPost("packages/purchase")]
        public async Task<IActionResult> PurchasePackage(BuyPackageDto request, CancellationToken cancellationToken)
        {
            try
            {
                await _packageService.PurchasePackageAsync(
                    request,
                    cancellationToken);

                return Ok("Package purchased successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
