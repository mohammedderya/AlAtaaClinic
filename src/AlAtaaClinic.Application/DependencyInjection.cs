using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Application.Features.Appointments;
using AlAtaaClinic.Application.Features.Auth;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Departments;
using AlAtaaClinic.Application.Features.CharityCases;
using AlAtaaClinic.Application.Features.Doctors;
using AlAtaaClinic.Application.Features.Invoices;
using AlAtaaClinic.Application.Features.Medicines;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Application.Features.StaffMembers;
using AlAtaaClinic.Application.Features.Visits;
using AlAtaaClinic.Application.Security;
using Microsoft.Extensions.DependencyInjection;

namespace AlAtaaClinic.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IExceptionHandler, ApplicationExceptionHandler>();
        services.AddScoped<IAuthorizationService, PermissionAuthorizationService>();

        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ICharityCaseService, CharityCaseService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IStaffMemberService, StaffMemberService>();
        services.AddScoped<IDoctorService, DoctorService>();

        services.AddValidators();
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreatePatientCommand>, CreatePatientCommandValidator>();
        services.AddScoped<IValidator<UpdatePatientCommand>, UpdatePatientCommandValidator>();
        services.AddScoped<IValidator<CreateAppointmentCommand>, CreateAppointmentCommandValidator>();
        services.AddScoped<IValidator<UpdateAppointmentCommand>, UpdateAppointmentCommandValidator>();
        services.AddScoped<IValidator<CreateVisitCommand>, CreateVisitCommandValidator>();
        services.AddScoped<IValidator<UpdateVisitCommand>, UpdateVisitCommandValidator>();
        services.AddScoped<IValidator<CreateInvoiceCommand>, CreateInvoiceCommandValidator>();
        services.AddScoped<IValidator<UpdateInvoiceCommand>, UpdateInvoiceCommandValidator>();
        services.AddScoped<IValidator<AddPaymentCommand>, AddPaymentCommandValidator>();
        services.AddScoped<IValidator<CreateCharityCaseCommand>, CreateCharityCaseCommandValidator>();
        services.AddScoped<IValidator<UpdateCharityCaseCommand>, UpdateCharityCaseCommandValidator>();
        services.AddScoped<IValidator<CreateMedicineCommand>, CreateMedicineCommandValidator>();
        services.AddScoped<IValidator<UpdateMedicineCommand>, UpdateMedicineCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<CreateBranchCommand>, CreateBranchCommandValidator>();
        services.AddScoped<IValidator<UpdateBranchCommand>, UpdateBranchCommandValidator>();
        services.AddScoped<IValidator<CreateDepartmentCommand>, CreateDepartmentCommandValidator>();
        services.AddScoped<IValidator<UpdateDepartmentCommand>, UpdateDepartmentCommandValidator>();
        services.AddScoped<IValidator<CreateStaffMemberCommand>, CreateStaffMemberCommandValidator>();
        services.AddScoped<IValidator<UpdateStaffMemberCommand>, UpdateStaffMemberCommandValidator>();
        services.AddScoped<IValidator<CreateDoctorCommand>, CreateDoctorCommandValidator>();
        services.AddScoped<IValidator<UpdateDoctorCommand>, UpdateDoctorCommandValidator>();
        services.AddScoped<IValidator<CreateUserAccountCommand>, CreateUserAccountCommandValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
        services.AddScoped(typeof(ValidationRunner<>));
        return services;
    }
}
