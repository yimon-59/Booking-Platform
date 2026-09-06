namespace Rezerv.Domain.Entities;

    public class UserPackage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int RemainingCredits { get; set; }
        public DateTime PurchasedAt { get; set; }
        public User User { get; set; } = null!;
        public Package Package { get; set; } = null!;
    }

