namespace CartService.Infrastructure.Messaging
{
    public class ProductUpdatedMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
    }
}