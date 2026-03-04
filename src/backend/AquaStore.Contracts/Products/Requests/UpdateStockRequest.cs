namespace AquaStore.Contracts.Products.Requests;

/// <summary>
/// Запрос на обновление остатков
/// </summary>
public sealed record UpdateStockRequest(int Quantity);

