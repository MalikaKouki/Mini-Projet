using System.Data;
using GestionCommerciale.Application.Orders;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Services;

public class OrderService : IOrderService
{
    private const decimal TaxRate = 0.19m;
    private readonly CommercialDbContext _dbContext;

    public OrderService(CommercialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Client)
            .Include(order => order.Lines)
                .ThenInclude(line => line.Product)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync(cancellationToken);

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Client)
            .Include(order => order.Lines)
                .ThenInclude(line => line.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        return order is null ? null : ToDto(order);
    }

    public async Task<OrderOperationResult> CreateAsync(
        OrderWriteRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == request.ClientId, cancellationToken);

        if (client is null)
        {
            return Failure(OrderOperationStatus.ClientNotFound);
        }

        var productResult = await GetProductsForRequestAsync(
            request.Lines,
            cancellationToken);

        if (productResult.Status != OrderOperationStatus.Success)
        {
            return Failure(productResult.Status, productResult.Message);
        }

        var order = new Order
        {
            OrderNumber = CreateOrderNumber(),
            ClientId = client.Id,
            Client = client,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatuses.Draft
        };

        AddLines(order, request.Lines, productResult.Products);
        RecalculateTotals(order);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OrderOperationResult(OrderOperationStatus.Success, ToDto(order));
    }

    public async Task<OrderOperationResult> UpdateAsync(
        int id,
        OrderWriteRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var order = await _dbContext.Orders
            .Include(order => order.Lines)
                .ThenInclude(line => line.Product)
            .Include(order => order.Client)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
        {
            return Failure(OrderOperationStatus.NotFound);
        }

        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == request.ClientId, cancellationToken);

        if (client is null)
        {
            return Failure(OrderOperationStatus.ClientNotFound);
        }

        if (order.Status == OrderStatuses.Validated)
        {
            foreach (var oldLine in order.Lines)
            {
                oldLine.Product.StockQuantity += oldLine.Quantity;
            }
        }

        var productResult = await GetProductsForRequestAsync(
            request.Lines,
            cancellationToken);

        if (productResult.Status != OrderOperationStatus.Success)
        {
            return Failure(productResult.Status, productResult.Message);
        }

        var requestedProductIds = request.Lines
            .Select(line => line.ProductId)
            .ToHashSet();

        foreach (var oldLine in order.Lines.ToList())
        {
            if (!requestedProductIds.Contains(oldLine.ProductId))
            {
                _dbContext.OrderLines.Remove(oldLine);
                order.Lines.Remove(oldLine);
            }
        }

        order.ClientId = client.Id;
        order.Client = client;

        foreach (var requestedLine in request.Lines)
        {
            var product = productResult.Products[requestedLine.ProductId];
            var existingLine = order.Lines
                .FirstOrDefault(line => line.ProductId == requestedLine.ProductId);

            if (existingLine is null)
            {
                order.Lines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = requestedLine.Quantity,
                    UnitPrice = product.UnitPrice
                });
            }
            else
            {
                existingLine.Quantity = requestedLine.Quantity;
                existingLine.UnitPrice = product.UnitPrice;
            }
        }

        RecalculateTotals(order);

        if (order.Status == OrderStatuses.Validated)
        {
            foreach (var line in order.Lines)
            {
                line.Product.StockQuantity -= line.Quantity;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new OrderOperationResult(OrderOperationStatus.Success, ToDto(order));
    }

    public async Task<OrderOperationStatus> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var order = await _dbContext.Orders
            .Include(order => order.Lines)
                .ThenInclude(line => line.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
        {
            return OrderOperationStatus.NotFound;
        }

        if (order.Status == OrderStatuses.Validated)
        {
            foreach (var line in order.Lines)
            {
                line.Product.StockQuantity += line.Quantity;
            }
        }

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return OrderOperationStatus.Success;
    }

    public async Task<OrderOperationResult> ValidateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var order = await _dbContext.Orders
            .Include(order => order.Lines)
                .ThenInclude(line => line.Product)
            .Include(order => order.Client)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
        {
            return Failure(OrderOperationStatus.NotFound);
        }

        if (order.Status != OrderStatuses.Draft)
        {
            return Failure(OrderOperationStatus.InvalidState);
        }

        if (order.Lines.Count == 0)
        {
            return Failure(OrderOperationStatus.EmptyOrder);
        }

        var insufficientLine = order.Lines
            .FirstOrDefault(line => line.Quantity > line.Product.StockQuantity);

        if (insufficientLine is not null)
        {
            return Failure(
                OrderOperationStatus.InsufficientStock,
                $"Stock insuffisant pour le produit {insufficientLine.Product.Name}.");
        }

        foreach (var line in order.Lines)
        {
            line.Product.StockQuantity -= line.Quantity;
        }

        order.Status = OrderStatuses.Validated;
        RecalculateTotals(order);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new OrderOperationResult(OrderOperationStatus.Success, ToDto(order));
    }

    private async Task<ProductLookupResult> GetProductsForRequestAsync(
        IReadOnlyCollection<OrderLineWriteRequest> requestedLines,
        CancellationToken cancellationToken)
    {
        if (requestedLines.Count == 0)
        {
            return ProductLookupFailure(OrderOperationStatus.EmptyOrder);
        }

        var productIds = requestedLines
            .Select(line => line.ProductId)
            .ToList();

        if (productIds.Count != productIds.Distinct().Count())
        {
            return ProductLookupFailure(OrderOperationStatus.DuplicateProducts);
        }

        var products = await _dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        if (products.Count != productIds.Count)
        {
            return ProductLookupFailure(OrderOperationStatus.ProductNotFound);
        }

        foreach (var requestedLine in requestedLines)
        {
            var product = products[requestedLine.ProductId];
            if (requestedLine.Quantity > product.StockQuantity)
            {
                return ProductLookupFailure(
                    OrderOperationStatus.InsufficientStock,
                    $"Stock insuffisant pour le produit {product.Name}.");
            }
        }

        return new ProductLookupResult(OrderOperationStatus.Success, products);
    }

    private static void AddLines(
        Order order,
        IEnumerable<OrderLineWriteRequest> requestedLines,
        IReadOnlyDictionary<int, Product> products)
    {
        foreach (var requestedLine in requestedLines)
        {
            var product = products[requestedLine.ProductId];
            order.Lines.Add(new OrderLine
            {
                ProductId = product.Id,
                Product = product,
                Quantity = requestedLine.Quantity,
                UnitPrice = product.UnitPrice
            });
        }
    }

    private static void RecalculateTotals(Order order)
    {
        order.TotalHT = order.Lines.Sum(line => line.Quantity * line.UnitPrice);
        order.TotalTTC = decimal.Round(
            order.TotalHT * (1 + TaxRate),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static string CreateOrderNumber()
    {
        return $"CMD-{Guid.NewGuid():N}"[..30];
    }

    private static OrderOperationResult Failure(
        OrderOperationStatus status,
        string? message = null)
    {
        return new OrderOperationResult(status, Message: message);
    }

    private static ProductLookupResult ProductLookupFailure(
        OrderOperationStatus status,
        string? message = null)
    {
        return new ProductLookupResult(status, new Dictionary<int, Product>(), message);
    }

    private static OrderDto ToDto(Order order)
    {
        var clientName = string.IsNullOrWhiteSpace(order.Client.FirstName)
            ? order.Client.LastName
            : $"{order.Client.FirstName} {order.Client.LastName}";

        var lines = order.Lines
            .Select(line => new OrderLineDto(
                line.ProductId,
                line.Product.Reference,
                line.Product.Name,
                line.Quantity,
                line.UnitPrice,
                line.Quantity * line.UnitPrice))
            .ToList();

        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.ClientId,
            clientName,
            order.OrderDate,
            order.Status,
            order.TotalHT,
            order.TotalTTC,
            lines);
    }

    private sealed record ProductLookupResult(
        OrderOperationStatus Status,
        IReadOnlyDictionary<int, Product> Products,
        string? Message = null);
}
