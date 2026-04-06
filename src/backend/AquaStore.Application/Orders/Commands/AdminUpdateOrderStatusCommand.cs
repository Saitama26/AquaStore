using AquaStore.Domain.Enums;
using AquaStore.Domain.Errors;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Products;
using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;

namespace AquaStore.Application.Orders.Commands;

/// <summary>
/// Обновление статуса заказа администратором
/// </summary>
public sealed record AdminUpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status) : ICommand;

internal sealed class AdminUpdateOrderStatusCommandHandler : ICommandHandler<AdminUpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminUpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AdminUpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (order.Status == request.Status)
        {
            return Result.Success();
        }

        if (request.Status == OrderStatus.Cancelled)
        {
            if (!order.CanBeCancelled)
            {
                return Result.Failure(OrderErrors.CannotCancel);
            }

            await ReturnItemsToStockAsync(order, cancellationToken);
            order.Cancel();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition(order.Status.ToString(), request.Status.ToString()));
        }

        if (request.Status == OrderStatus.Pending || request.Status < order.Status)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition(order.Status.ToString(), request.Status.ToString()));
        }

        while (order.Status < request.Status)
        {
            switch (order.Status)
            {
                case OrderStatus.Pending:
                    order.Confirm();
                    break;

                case OrderStatus.Confirmed:
                    order.Process();
                    break;

                case OrderStatus.Processing:
                    order.Ship();
                    break;

                case OrderStatus.Shipped:
                    order.Deliver();
                    break;

                default:
                    return Result.Failure(OrderErrors.InvalidStatusTransition(order.Status.ToString(), request.Status.ToString()));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task ReturnItemsToStockAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            if (!item.ProductId.HasValue)
            {
                continue;
            }

            var product = await _productRepository.GetByIdAsync(item.ProductId.Value, cancellationToken);
            product?.AddStock(item.Quantity);
        }
    }
}
