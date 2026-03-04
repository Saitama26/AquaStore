using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Brands;

namespace AquaStore.Application.Brands.Queries;

/// <summary>
/// Запрос на получение всех брендов
/// </summary>
public sealed record GetBrandsQuery : IQuery<IReadOnlyList<BrandResponse>>;

public sealed record BrandResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Country,
    string? LogoUrl,
    string? WebsiteUrl,
    bool IsActive);

internal sealed class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, IReadOnlyList<BrandResponse>>
{
    private readonly IBrandRepository _brandRepository;

    public GetBrandsQueryHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Result<IReadOnlyList<BrandResponse>>> Handle(
        GetBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var brands = await _brandRepository.GetAllAsync(cancellationToken);

        var response = brands
            .Where(b => b.IsActive)
            .Select(b => new BrandResponse(
                b.Id,
                b.Name,
                b.Slug,
                b.Description,
                b.Country,
                b.LogoUrl,
                b.WebsiteUrl,
                b.IsActive))
            .ToList();

        return response;
    }
}

