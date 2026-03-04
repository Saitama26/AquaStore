using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Users;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Auth.Commands;

/// <summary>
/// Команда обновления профиля пользователя
/// </summary>
public sealed record UpdateUserProfileCommand(
    string FirstName,
    string LastName,
    string? Phone) : ICommand;

internal sealed class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(_currentUserService.UserId.Value));
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.Phone);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

