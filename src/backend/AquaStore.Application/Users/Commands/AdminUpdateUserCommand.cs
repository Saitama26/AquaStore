using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Errors;
using AquaStore.Domain.Users;

namespace AquaStore.Application.Users.Commands;

/// <summary>
/// Команда обновления пользователя из админ-панели
/// </summary>
public sealed record AdminUpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool IsActive) : ICommand;

internal sealed class AdminUpdateUserCommandHandler : ICommandHandler<AdminUpdateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminUpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AdminUpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.Phone);

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            role = user.Role;
        }

        user.SetRole(role);

        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

