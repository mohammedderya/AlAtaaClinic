using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Branches;

public static class BranchMapper
{
    public static BranchDto ToDto(this Branch branch)
    {
        return new BranchDto(
            branch.Id,
            branch.BranchCode,
            branch.ArabicName,
            branch.EnglishName,
            branch.PhoneNumber,
            branch.AddressLine,
            branch.IsActive);
    }

    public static BranchListDto ToListDto(this Branch branch)
    {
        return new BranchListDto(
            branch.Id,
            branch.BranchCode,
            branch.ArabicName,
            branch.PhoneNumber,
            branch.IsActive);
    }
}
