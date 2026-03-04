using Common.Domain.Errors;

namespace Common.Application.Exceptions;

/// <summary>
/// Исключение валидации
/// </summary>
public sealed class ValidationException : ApplicationException
{
    public IReadOnlyCollection<Error> Errors { get; }

    public ValidationException(IEnumerable<Error> errors)
        : base(
            "Validation.Error",
            "One or more validation errors occurred")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public ValidationException(Error error)
        : this([error])
    {
    }
}

