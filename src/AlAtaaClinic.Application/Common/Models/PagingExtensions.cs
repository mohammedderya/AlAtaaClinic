namespace AlAtaaClinic.Application.Common.Models;

public static class PagingExtensions
{
    public static PagedResult<TTarget> Map<TSource, TTarget>(
        this PagedResult<TSource> source,
        Func<TSource, TTarget> mapper)
    {
        return new PagedResult<TTarget>(
            source.Items.Select(mapper).ToList(),
            source.TotalCount,
            source.PageNumber,
            source.PageSize);
    }
}
