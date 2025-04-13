using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CartService.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string cartId)
        {
            var cart = await _cartRepository.GetCartAsync(cartId);
            return cart?.Items ?? new List<CartItem>();
        }

        public async Task AddItemAsync(string cartId, CartItem item)
        {
            var cart = await _cartRepository.GetCartAsync(cartId) ?? new Cart { Id = cartId };

            var existingItem = cart.Items.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Items.Add(item);
            }

            await _cartRepository.SaveCartAsync(cart);
            _logger.LogInformation($"Item {item.Id} added to cart {cartId}.");
        }

        public async Task RemoveItemAsync(string cartId, int itemId)
        {
            var cart = await _cartRepository.GetCartAsync(cartId);
            if (cart == null)
                return;

            var existingItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (existingItem != null)
            {
                existingItem.Quantity--;
                if (existingItem.Quantity <= 0)
                {
                    cart.Items.Remove(existingItem);
                }
                await _cartRepository.SaveCartAsync(cart);
                _logger.LogInformation($"Item {itemId} quantity decreased in cart {cartId}.");
            }
        }
    }
}
