using CartService.Domain.Entities;

namespace CartService.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartAsync(string cartId);
        Task SaveCartAsync(Cart cart);
        Task<IEnumerable<Cart>> GetAllCartsAsync();
    }
}
