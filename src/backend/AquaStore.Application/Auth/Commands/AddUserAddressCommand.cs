using AquaStore.Domain.Errors;
using AquaStore.Domain.Users;
using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;

namespace AquaStore.Application.Auth.Commands;

public sealed record AddUserAddressCommand(
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode,
    bool IsDefault = false) : ICommand;

internal sealed class AddUserAddressCommandHandler : ICommandHandler<AddUserAddressCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddUserAddressCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        var user = await _userRepository.GetByIdWithAddressesAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(_currentUserService.UserId.Value));
        }

        var isDefault = request.IsDefault || user.Addresses.Count == 0;
        user.AddAddress(
            request.City,
            request.Street,
            request.Building,
            request.Apartment,
            request.PostalCode,
            isDefault);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
