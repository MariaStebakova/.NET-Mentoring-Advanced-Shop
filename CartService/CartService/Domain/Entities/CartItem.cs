namespace CartService.Domain.Entities
{
    /// <summary>
    /// Represents a single item in the shopping cart.
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// ID of the item in the external system.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional image URL.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Alt text for the image.
        /// </summary>
        public string? ImageAltText { get; set; }

        /// <summary>
        /// Price of the item.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity of the item in the cart.
        /// </summary>
        public int Quantity { get; set; }
    }
}