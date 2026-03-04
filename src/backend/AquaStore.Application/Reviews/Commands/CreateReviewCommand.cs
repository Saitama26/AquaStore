using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Errors;
using Common.Domain.Results;
using AquaStore.Contracts.Reviews.Responses;
using AquaStore.Domain.Errors;
using AquaStore.Domain.Products;
using AquaStore.Domain.Reviews;
using AquaStore.Domain.Users;

namespace AquaStore.Application.Reviews.Commands;

public sealed record CreateReviewCommand(
    Guid ProductId,
    int Rating,
    string? Comment) : ICommand<ReviewResponse>;

internal sealed class CreateReviewCommandHandler
    : ICommandHandler<CreateReviewCommand, ReviewResponse>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewResponse>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            return Error.Unauthorized("Review.Unauthorized", "Authentication required");
        }

        var userId = _currentUserService.UserId.Value;

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(request.ProductId);
        }

        if (await _reviewRepository.UserHasReviewedProductAsync(userId, request.ProductId, cancellationToken))
        {
            return ReviewErrors.AlreadyReviewed(request.ProductId);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(userId);
        }

        Review review;
        try
        {
            review = Review.Create(request.ProductId, userId, request.Rating, request.Comment);
        }
        catch (ArgumentOutOfRangeException)
        {
            return ReviewErrors.InvalidRating;
        }

        review.Approve();

        _reviewRepository.Add(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewResponse(
            review.Id,
            review.ProductId,
            review.UserId,
            user.FullName,
            review.Rating.Value,
            review.Comment,
            review.IsApproved,
            review.CreatedAtUtc);
    }
}

