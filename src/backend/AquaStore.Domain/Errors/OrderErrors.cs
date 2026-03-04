using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с заказами
/// </summary>
public static class OrderErrors
{
    public static Error NotFound(Guid orderId) =>
        Error.NotFound("Order.NotFound", $"Order with ID '{orderId}' was not found");

    public static Error NotFoundByNumber(string orderNumber) =>
        Error.NotFound("Order.NotFoundByNumber", $"Order with number '{orderNumber}' was not found");

    public static Error EmptyCart =>
        Error.Validation("Order.EmptyCart", "Cannot create order from empty cart");

    public static Error InvalidStatusTransition(string from, string to) =>
        Error.Validation("Order.InvalidStatusTransition",
            $"Cannot transition order status from '{from}' to '{to}'");

    public static Error CannotCancel =>
        Error.Validation("Order.CannotCancel", "Order cannot be cancelled in its current state");

    public static Error AlreadyPaid =>
        Error.Conflict("Order.AlreadyPaid", "Order has already been paid");

    public static Error NotPaid =>
        Error.Validation("Order.NotPaid", "Order must be paid before it can be processed");
}

