using CartService.Domain.Entities;

namespace CartService.Application.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync(string cartId);
        Task AddItemAsync(string cartId, CartItem item);
        Task RemoveItemAsync(string cartId, int itemId);
    }
}
