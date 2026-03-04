using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Brands;
using AquaStore.Domain.Errors;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Application.Brands.Commands;

/// <summary>
/// Команда обновления бренда
/// </summary>
public sealed record UpdateBrandCommand(
    Guid BrandId,
    string Name,
    string? Description,
    string? Country,
    string? LogoUrl,
    string? WebsiteUrl) : ICommand;

internal sealed class UpdateBrandCommandHandler : ICommandHandler<UpdateBrandCommand>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand is null)
        {
            return Result.Failure(BrandErrors.NotFound(request.BrandId));
        }

        var newSlug = Slug.Create(request.Name);
        if (await _brandRepository.SlugExistsAsync(newSlug.Value, request.BrandId, cancellationToken))
        {
            return Result.Failure(BrandErrors.SlugAlreadyExists(newSlug.Value));
        }

        brand.Update(
            request.Name,
            request.Description,
            request.Country,
            request.LogoUrl,
            request.WebsiteUrl);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

