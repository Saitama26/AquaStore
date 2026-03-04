using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Reviews;
using AquaStore.Contracts.Common;
using AquaStore.Contracts.Reviews.Responses;

namespace AquaStore.Application.Reviews.Queries;

public sealed record GetReviewsByProductIdQuery(
    Guid ProductId,
    int PageNumber,
    int PageSize) : IQuery<PagedResponse<ReviewResponse>>;

internal sealed class GetReviewsByProductIdQueryHandler
    : IQueryHandler<GetReviewsByProductIdQuery, PagedResponse<ReviewResponse>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewsByProductIdQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<PagedResponse<ReviewResponse>>> Handle(
        GetReviewsByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _reviewRepository.GetPagedByProductIdAsync(
            request.ProductId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var reviews = items
            .Select(r => new ReviewResponse(
                r.Id,
                r.ProductId,
                r.UserId,
                r.User.FullName,
                r.Rating.Value,
                r.Comment,
                r.IsApproved,
                r.CreatedAtUtc))
            .ToList();

        return new PagedResponse<ReviewResponse>
        {
            Items = reviews,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber * request.PageSize < totalCount
        };
    }
}

