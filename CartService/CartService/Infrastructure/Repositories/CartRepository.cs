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

        public async Task<Cart?> GetCartAsync(string cartId)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var carts = db.GetCollection<Cart>("carts");
                return carts.FindById(cartId);
            }
            catch (LiteException ex)
            {
                _logger.LogError(ex, $"Error retrieving cart {cartId}.");
                throw;
            }
        }

        public async Task SaveCartAsync(Cart cart)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var carts = db.GetCollection<Cart>("carts");
                carts.Upsert(cart);
            }
            catch (LiteException ex)
            {
                _logger.LogError(ex, $"Error saving cart {cart.Id}.");
                throw;
            }
        }

        public async Task<IEnumerable<Cart>> GetAllCartsAsync()
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<Cart>("carts");
            return collection.FindAll().ToList();
        }
    }
}
