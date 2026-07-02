namespace AlAtaaClinic.Application.Common.Models;

public sealed record OperationResult(bool Succeeded, string? Message = null)
{
    public static OperationResult Success(string? message = null)
    {
        return new OperationResult(true, message);
    }
}
