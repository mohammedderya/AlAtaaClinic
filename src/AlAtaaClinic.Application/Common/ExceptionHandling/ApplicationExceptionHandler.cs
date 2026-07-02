using AlAtaaClinic.Application.Common.Exceptions;

namespace AlAtaaClinic.Application.Common.ExceptionHandling;

public sealed class ApplicationExceptionHandler : IExceptionHandler
{
    public ErrorDto Handle(Exception exception)
    {
        return exception switch
        {
            ValidationFailedException validation => HandleValidation(validation),
            NotFoundException notFound => Create("not_found", notFound.Message),
            ForbiddenException forbidden => Create("forbidden", forbidden.Message),
            UnauthorizedAppException unauthorized => Create("unauthorized", unauthorized.Message),
            ConflictException conflict => Create("conflict", conflict.Message),
            _ => Create("unexpected_error", GetInnerMessage(exception))
        };
    }

    private static ErrorDto HandleValidation(ValidationFailedException exception)
    {
        var details = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());

        return new ErrorDto("validation_failed", exception.Message, details);
    }

    private static ErrorDto Create(string code, string message)
    {
        return new ErrorDto(code, message, new Dictionary<string, string[]>());
    }

    private static string GetInnerMessage(Exception exception)
    {
        while (exception.InnerException is not null)
            exception = exception.InnerException;
        return exception.Message;
    }
}
