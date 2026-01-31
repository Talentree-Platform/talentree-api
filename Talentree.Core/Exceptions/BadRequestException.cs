namespace Talentree.Core.Exceptions;

/// <summary>
/// Exception thrown when request validation fails (400 Bad Request)
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when resource not found (404 Not Found)
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.") { }
}

/// <summary>
/// Exception thrown when authentication fails (401 Unauthorized)
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException() : base("Unauthorized access.") { }
}

/// <summary>
/// Exception thrown when user lacks permissions (403 Forbidden)
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException() : base("Access forbidden.") { }
}

/// <summary>
/// Exception thrown when validation fails
/// Used by FluentValidation
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }
}