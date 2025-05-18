using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Infrastructure.Messaging;
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
            {
                throw new InvalidOperationException("Cart not found");
            }

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

        public async Task ApplyProductUpdate(ProductUpdatedMessage productUpdate)
        {
            var allCarts = await _cartRepository.GetAllCartsAsync();
            foreach (var cart in allCarts)
            {
                var item = cart.Items.FirstOrDefault(i => i.Id == productUpdate.Id);
                if (item != null)
                {
                    await UpdateCartItemAsync(cart, item, productUpdate);
                }
            }
        }

        private async Task UpdateCartItemAsync(Cart cart, CartItem item, ProductUpdatedMessage productUpdate)
        {
            if (productUpdate.Amount == 0)
            {
                cart.Items.Remove(item);
                _logger.LogInformation($"Removed item {item.Id} from cart {cart.Id}");
            }
            else
            {
                if (item.Quantity > productUpdate.Amount)
                {
                    item.Quantity = productUpdate.Amount;
                    _logger.LogInformation($"Reduced quantity of item {item.Id} in cart {cart.Id} to {item.Quantity}");
                }
                item.Name = productUpdate.Name;
                item.Price = productUpdate.Price;
                item.ImageUrl = productUpdate.ImageUrl;
            }
            await _cartRepository.SaveCartAsync(cart);
        }
    }
}
