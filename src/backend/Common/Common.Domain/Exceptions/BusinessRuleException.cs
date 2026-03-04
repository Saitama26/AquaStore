namespace Common.Domain.Exceptions;

/// <summary>
/// Исключение при нарушении бизнес-правила
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message)
        : base(code, message)
    {
    }
}

