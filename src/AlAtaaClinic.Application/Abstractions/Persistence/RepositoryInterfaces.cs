using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Billing;
using AlAtaaClinic.Domain.Charity;
using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Inventory;
using AlAtaaClinic.Domain.Security;

namespace AlAtaaClinic.Application.Abstractions.Persistence;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByFileNumberAsync(string fileNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<Patient>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default);
}

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<PagedResult<Appointment>> SearchAsync(long? doctorId, DateTime? from, DateTime? to, PageRequest request, CancellationToken cancellationToken = default);
}

public interface IVisitRepository : IRepository<Visit>
{
    Task<Visit?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<Visit>> SearchAsync(long? patientId, long? doctorId, DateTime? from, DateTime? to, PageRequest request, CancellationToken cancellationToken = default);
}

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<Invoice>> SearchAsync(long? patientId, string? status, PageRequest request, CancellationToken cancellationToken = default);
}

public interface ICharityCaseRepository : IRepository<CharityCase>
{
    Task<PagedResult<CharityCase>> SearchAsync(long? patientId, string? caseNumber, PageRequest request, CancellationToken cancellationToken = default);
}

public interface IMedicineRepository : IRepository<Medicine>
{
    Task<Medicine?> GetByCodeAsync(string medicineCode, CancellationToken cancellationToken = default);
    Task<PagedResult<Medicine>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default);
}

public interface IBranchRepository : IRepository<Branch>
{
    Task<Branch?> GetByCodeAsync(string branchCode, CancellationToken cancellationToken = default);
    Task<PagedResult<Branch>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository : IRepository<Department>
{
    Task<PagedResult<Department>> SearchAsync(string? searchText, long? branchId, PageRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(long branchId, string arabicName, long? excludeId, CancellationToken cancellationToken = default);
}

public interface IUserAccountRepository : IRepository<UserAccount>
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetWithPermissionsAsync(long userAccountId, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}

public interface IStaffMemberRepository : IRepository<StaffMember>
{
    Task<StaffMember?> GetByCodeAsync(string staffCode, CancellationToken cancellationToken = default);
    Task<StaffMember?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<StaffMember>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffMember>> ListActiveAsync(CancellationToken cancellationToken = default);
}

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Doctor?> GetWithStaffAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Doctor>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByStaffMemberIdAsync(long staffMemberId, CancellationToken cancellationToken = default);
    Task AddAsync(Doctor entity, CancellationToken cancellationToken = default);
    void Update(Doctor entity);
    void Remove(Doctor entity);
}
