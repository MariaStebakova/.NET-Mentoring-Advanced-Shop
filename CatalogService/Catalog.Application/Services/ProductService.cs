using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMessagePublisher _messagePublisher;

    public ProductService(IProductRepository repository, IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product ?? throw new KeyNotFoundException($"Product with ID {id} not found.");
    }

    public Task<IEnumerable<Product>> GetAllAsync() => _repository.GetAllAsync();

    public async Task<Product> AddAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || product.Name.Length > 50)
            throw new ArgumentException("Invalid product name.");
        if (product.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.");
        if (product.Amount < 0)
            throw new ArgumentException("Amount must be a positive integer.");
        if (string.IsNullOrWhiteSpace(product.Currency))
            throw new ArgumentException("Currency must be specified.");

        await _repository.AddAsync(product);
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || product.Name.Length > 50)
            throw new ArgumentException("Invalid product name.");
        if (product.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.");
        if (product.Amount < 0)
            throw new ArgumentException("Amount must be a positive integer.");
        if (string.IsNullOrWhiteSpace(product.Currency))
            throw new ArgumentException("Currency must be specified.");

        await _repository.UpdateAsync(product);

        var message = new ProductUpdatedMessage
        {
            Id = product.Id,
            Name = product.Name,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            Amount = product.Amount
        };

        await _messagePublisher.PublishProductUpdatedAsync(message);
    }

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}