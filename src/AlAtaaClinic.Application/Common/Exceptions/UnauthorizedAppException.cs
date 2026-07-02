namespace AlAtaaClinic.Application.Common.Exceptions;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException()
        : base("Authentication is required.")
    {
    }
}
