namespace AlAtaaClinic.Application.Common.Models;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 25)
{
    public int Skip => (NormalizedPageNumber - 1) * NormalizedPageSize;
    public int NormalizedPageNumber => PageNumber < 1 ? 1 : PageNumber;
    public int NormalizedPageSize => PageSize is < 1 or > 200 ? 25 : PageSize;
}
