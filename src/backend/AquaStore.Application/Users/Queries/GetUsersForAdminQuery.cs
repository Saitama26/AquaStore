using Common.Application.Abstractions.Messaging;
using Common.Application.Models;
using Common.Domain.Results;
using AquaStore.Contracts.Users.Responses;
using AquaStore.Domain.Users;

namespace AquaStore.Application.Users.Queries;

/// <summary>
/// Запрос на получение списка пользователей для админ-панели
/// </summary>
public sealed record GetUsersForAdminQuery(
    int PageNumber = 1,
    int PageSize = 50) : IQuery<PagedResult<AdminUserResponse>>;

internal sealed class GetUsersForAdminQueryHandler
    : IQueryHandler<GetUsersForAdminQuery, PagedResult<AdminUserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersForAdminQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<AdminUserResponse>>> Handle(
        GetUsersForAdminQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var totalCount = users.Count;

        var items = users
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new AdminUserResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Phone?.ToString(),
                u.Role.ToString(),
                u.IsActive,
                u.CreatedAtUtc))
            .ToList();

        var paged = new PagedResult<AdminUserResponse>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return paged;
    }
}

