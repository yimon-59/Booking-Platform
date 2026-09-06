using Rezerv.Application.DTOs.Packages;

namespace Rezerv.Application.Interfaces
{
    public interface IPackageService
    {
        Task<List<PackageDto>> GetPackagesAsync(CancellationToken cancellationToken = default);

        Task<List<UserPackageDto>> GetUserPackagesAsync(int userId,CancellationToken cancellationToken = default);

        Task PurchasePackageAsync(BuyPackageDto package, CancellationToken cancellationToken = default);
    }
}
