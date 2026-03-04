using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Products;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Orders.Commands;

/// <summary>
/// Команда отмены заказа
/// </summary>
public sealed record CancelOrderCommand(Guid OrderId) : ICommand;

internal sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        // Проверяем права
        if (_currentUserService.UserId != order.UserId && !_currentUserService.IsInRole("Admin"))
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (!order.CanBeCancelled)
        {
            return Result.Failure(OrderErrors.CannotCancel);
        }

        // Возвращаем товары на склад
        foreach (var item in order.Items)
        {
            // ProductId может быть null если продукт уже удален
            if (item.ProductId.HasValue)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId.Value, cancellationToken);
                product?.AddStock(item.Quantity);
            }
        }

        order.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

