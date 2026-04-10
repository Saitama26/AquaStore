using AquaStore.Domain.Brands;
using AquaStore.Domain.ValueObjects;
using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;

namespace AquaStore.Application.Brands.Commands;

public sealed record BulkCreateBrandsCommand(
    IReadOnlyList<BulkCreateBrandItemCommand> Brands) : ICommand;

public sealed record BulkCreateBrandItemCommand(
    string Name,
    string? Description,
    string? Country,
    string? LogoUrl,
    string? WebsiteUrl);

internal sealed class BulkCreateBrandsCommandHandler : ICommandHandler<BulkCreateBrandsCommand>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkCreateBrandsCommandHandler(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(BulkCreateBrandsCommand request, CancellationToken cancellationToken)
    {
        var existingBrands = await _brandRepository.GetAllAsync(cancellationToken);
        var brandSlugs = existingBrands
            .Select(b => b.Slug.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasChanges = false;

        foreach (var item in request.Brands)
        {
            var slug = Slug.Create(item.Name).Value;
            if (!brandSlugs.Add(slug))
            {
                continue;
            }

            var brand = Brand.Create(item.Name.Trim(), item.Description, item.Country, item.LogoUrl, item.WebsiteUrl);
            _brandRepository.Add(brand);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
