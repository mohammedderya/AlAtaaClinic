namespace AlAtaaClinic.Application.Common.Models;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public static PagedResult<T> Empty(PageRequest request)
    {
        return new PagedResult<T>([], 0, request.NormalizedPageNumber, request.NormalizedPageSize);
    }
}
