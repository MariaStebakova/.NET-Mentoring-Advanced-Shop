namespace CartService.Domain.Entities
{
    /// <summary>
    /// Represents a shopping cart with a unique identifier and a list of items.
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// Unique identifier of the cart.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// List of items in the cart.
        /// </summary>
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}