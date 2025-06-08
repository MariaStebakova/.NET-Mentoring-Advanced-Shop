using CartService.Application.Interfaces;
using CartService.Domain.Entities;

using LiteDB;

using Microsoft.Extensions.Logging;

namespace CartService.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(string connectionString, ILogger<CartRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public Task<Cart?> GetCartAsync(string cartId)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var carts = db.GetCollection<Cart>("carts");
                var cart = carts.FindById(cartId);
                return Task.FromResult<Cart?>(cart);
            }
            catch (LiteException ex)
            {
                _logger.LogError(ex, "Error retrieving cart {cartId}.", cartId);
                throw new InvalidOperationException($"Failed to retrieve cart with ID '{cartId}'.", ex);
            }
        }

        public Task SaveCartAsync(Cart cart)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var carts = db.GetCollection<Cart>("carts");
                carts.Upsert(cart);
                return Task.CompletedTask;
            }
            catch (LiteException ex)
            {
                _logger.LogError(ex, "Error saving cart {CartId}.", cart.Id);
                throw new InvalidOperationException($"Failed to save cart with ID '{cart.Id}'.", ex);
            }
        }

        public Task<IEnumerable<Cart>> GetAllCartsAsync()
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<Cart>("carts");
            var carts = collection.FindAll().ToList();
            return Task.FromResult<IEnumerable<Cart>>(carts);
        }
    }
}