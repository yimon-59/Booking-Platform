namespace Rezerv.Domain.Entities;

public class Package
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public int TotalCredits { get; set; }

    public DateTime ExpiryDate { get; set; }

    public Business Business { get; set; } = null!;
    public ICollection<UserPackage> UserPackages { get; set; }
      = new List<UserPackage>();
}