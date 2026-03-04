using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Brands;

namespace AquaStore.Application.Brands.Commands;

/// <summary>
/// Команда создания бренда
/// </summary>
public sealed record CreateBrandCommand(
    string Name,
    string? Description = null,
    string? Country = null,
    string? LogoUrl = null,
    string? WebsiteUrl = null) : ICommand<Guid>;

internal sealed class CreateBrandCommandHandler : ICommandHandler<CreateBrandCommand, Guid>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBrandCommandHandler(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = Brand.Create(
            request.Name,
            request.Description,
            request.Country,
            request.LogoUrl,
            request.WebsiteUrl);

        _brandRepository.Add(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return brand.Id;
    }
}

