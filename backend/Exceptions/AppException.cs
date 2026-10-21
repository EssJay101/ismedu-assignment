namespace Ismedu.Api.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public Dictionary<string, string[]>? Errors { get; }

    public AppException(int statusCode, string message, Dictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message, Dictionary<string, string[]>? errors = null)
        : base(StatusCodes.Status400BadRequest, message, errors) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(StatusCodes.Status404NotFound, message) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message, Dictionary<string, string[]>? errors = null)
        : base(StatusCodes.Status409Conflict, message, errors) { }
}