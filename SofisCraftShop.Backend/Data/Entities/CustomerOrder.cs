namespace SofisCraftShop.Backend.Data.Entities
{
    public class CustomerOrder
    {
        public int Id { get; set; }
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }

        public string CustomerName { get; set; } = "Townsperson";
        public string RequestedItemId { get; set; } = string.Empty;
        public int QuantityRequired { get; set; } = 1;
        public int RewardGold { get; set; } = 50;
        public int RewardXp { get; set; } = 20;
        public bool IsCompleted { get; set; } = false;
        public DateTime ExpirationTimeUtc { get; set; }
    }
}
