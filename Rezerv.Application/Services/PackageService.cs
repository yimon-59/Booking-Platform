using Microsoft.EntityFrameworkCore;
using Rezerv.Application.DTOs.Packages;
using Rezerv.Application.Interfaces;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services
{
    public class PackageService : IPackageService
    {
        IApplicationDbContext _context;

        public PackageService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PackageDto>> GetPackagesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Packages
           .AsNoTracking()
           .Where(x => x.ExpiryDate > DateTime.UtcNow)
           .Select(x => new PackageDto
           {
               Id = x.Id,
               Name = x.Name,
               BusinessId = x.BusinessId,
               BusinessName = x.Business.Name,
               TotalCredits = x.TotalCredits,
               ExpiryDate = x.ExpiryDate
           })
           .OrderBy(x => x.Id)
           .ToListAsync(cancellationToken);
        }

        public async Task<List<UserPackageDto>> GetUserPackagesAsync(
        int userId,
        CancellationToken cancellationToken = default)
        {
            return await _context.UserPackages
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId)
                .Select(x => new UserPackageDto
                {
                    Id = x.Id,
                    Name = x.Package.Name,
                    BusinessId = x.Package.BusinessId,
                    BusinessName = x.Package.Business.Name,
                    RemainingCredits = x.RemainingCredits,
                    ExpiryDate = x.Package.ExpiryDate
                })
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task PurchasePackageAsync(
        BuyPackageDto request,
        CancellationToken cancellationToken = default)
        {
            var userExists = await _context.Users
                .AnyAsync(x => x.Id == request.UserId, cancellationToken);

            if (!userExists)
            {
                throw new KeyNotFoundException(
                    "User does not exist.");
            }

            var selectedPackage = await _context.Packages
                .Where(x => x.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync(
                    x => x.Id == request.PackageId,
                    cancellationToken);

            if (selectedPackage is null)
            {
                throw new KeyNotFoundException(
                    "Package does not exist.");
            }

            var alreadyPurchased = await _context.UserPackages
                .AnyAsync(
                    x => x.UserId == request.UserId &&
                         x.PackageId == request.PackageId,
                    cancellationToken);

            if (alreadyPurchased)
            {
                throw new InvalidOperationException(
                    "User already has this package.");
            }

            var userPackage = new Domain.Entities.UserPackage
            {
                UserId = request.UserId,
                PackageId = request.PackageId,
                RemainingCredits = selectedPackage.TotalCredits,
                PurchasedAt = DateTime.UtcNow
            };

            _context.UserPackages.Add(userPackage);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
