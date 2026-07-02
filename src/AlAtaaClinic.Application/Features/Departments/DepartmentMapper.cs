using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Departments;

public static class DepartmentMapper
{
    public static DepartmentDto ToDto(this Department department)
    {
        return new DepartmentDto(
            department.Id,
            department.BranchId,
            department.Branch?.ArabicName ?? string.Empty,
            department.ArabicName,
            department.EnglishName,
            department.IsActive);
    }

    public static DepartmentListDto ToListDto(this Department department)
    {
        return new DepartmentListDto(
            department.Id,
            department.ArabicName,
            department.Branch?.ArabicName ?? string.Empty,
            department.IsActive);
    }
}
