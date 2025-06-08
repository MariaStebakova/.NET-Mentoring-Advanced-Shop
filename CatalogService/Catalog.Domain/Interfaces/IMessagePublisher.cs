using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;

public interface IMessagePublisher
{
    Task PublishProductUpdatedAsync(ProductUpdatedMessage item);
}