using CartService.Domain.Entities;
using CartService.Infrastructure.Messaging;

namespace CartService.Application.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync(string cartId);
        Task AddItemAsync(string cartId, CartItem item);
        Task RemoveItemAsync(string cartId, int itemId);
        Task ApplyProductUpdate(ProductUpdatedMessage productUpdate);
    }
}
