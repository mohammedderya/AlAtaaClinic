namespace AlAtaaClinic.Application.Common.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string permission)
        : base($"The current user is missing permission '{permission}'.")
    {
        Permission = permission;
    }

    public string Permission { get; }
}
