using AlAtaaClinic.Domain.Audit;
using AlAtaaClinic.Domain.Billing;
using AlAtaaClinic.Domain.Charity;
using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Inventory;
using AlAtaaClinic.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Persistence;

public sealed class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<ClinicalProcedure> Procedures => Set<ClinicalProcedure>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<MedicalOrder> MedicalOrders => Set<MedicalOrder>();
    public DbSet<MedicalResult> MedicalResults => Set<MedicalResult>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<DiscountApproval> DiscountApprovals => Set<DiscountApproval>();
    public DbSet<CharityCase> CharityCases => Set<CharityCase>();
    public DbSet<CharityCaseInvoice> CharityCaseInvoices => Set<CharityCaseInvoice>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCore(modelBuilder);
        ConfigureClinical(modelBuilder);
        ConfigureBilling(modelBuilder);
        ConfigureCharity(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureSecurity(modelBuilder);
        ConfigureAudit(modelBuilder);
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = utcNow;
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = utcNow;
        }
    }

    private static void ConfigureCore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("BranchId");
            entity.Property(x => x.BranchCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ArabicName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.BranchCode).IsUnique();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("DepartmentId");
            entity.Property(x => x.ArabicName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EnglishName).HasMaxLength(200);
            entity.HasIndex(x => new { x.BranchId, x.ArabicName }).IsUnique();
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
        });

        modelBuilder.Entity<StaffMember>(entity =>
        {
            entity.ToTable("StaffMembers", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("StaffMemberId");
            entity.Property(x => x.StaffCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullNameArabic).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.StaffCode).IsUnique();
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctors", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("DoctorId");
            entity.Property(x => x.Specialty).HasMaxLength(150).IsRequired();
            entity.HasOne(x => x.StaffMember).WithOne().HasForeignKey<Doctor>(x => x.StaffMemberId);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("PatientId");
            entity.Property(x => x.FileNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullNameArabic).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => x.FileNumber).IsUnique();
            entity.HasIndex(x => x.NationalId).IsUnique().HasFilter("[NationalId] IS NOT NULL");
            entity.HasIndex(x => x.FullNameArabic);
            entity.HasIndex(x => x.PhoneNumber);
        });
    }

    private static void ConfigureClinical(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("AppointmentId");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            entity.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
            entity.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId);
            entity.HasIndex(x => new { x.DoctorId, x.AppointmentStart });
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.ToTable("Visits", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("VisitId");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Appointment).WithOne().HasForeignKey<Visit>(x => x.AppointmentId);
            entity.HasMany(x => x.VitalSigns).WithOne(x => x.Visit).HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Diagnoses).WithOne(x => x.Visit).HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Procedures).WithOne(x => x.Visit).HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Prescriptions).WithOne(x => x.Visit).HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.MedicalOrders).WithOne(x => x.Visit).HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.PatientId, x.VisitDate });
            entity.HasIndex(x => new { x.DoctorId, x.VisitDate });
        });

        modelBuilder.Entity<VitalSign>(entity =>
        {
            entity.ToTable("VitalSigns", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("VitalSignId");
            entity.Property(x => x.Temperature).HasPrecision(18, 2);
            entity.Property(x => x.WeightKg).HasPrecision(18, 2);
            entity.Property(x => x.HeightCm).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.ToTable("Diagnoses", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("DiagnosisId");
        });

        modelBuilder.Entity<ClinicalProcedure>(entity =>
        {
            entity.ToTable("Procedures", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("ProcedureId");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.ToTable("Prescriptions", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("PrescriptionId");
            entity.HasMany(x => x.Items).WithOne(x => x.Prescription).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.ToTable("PrescriptionItems", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("PrescriptionItemId");
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
        });

        modelBuilder.Entity<MedicalOrder>(entity =>
        {
            entity.ToTable("MedicalOrders", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("MedicalOrderId");
            entity.Property(x => x.OrderType).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasMany(x => x.Results).WithOne(x => x.MedicalOrder).HasForeignKey(x => x.MedicalOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MedicalResult>(entity =>
        {
            entity.ToTable("MedicalResults", "clinical");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("MedicalResultId");
        });
    }

    private static void ConfigureBilling(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("InvoiceId");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.GrossAmount).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.NetAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            entity.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
            entity.HasOne(x => x.Visit).WithOne().HasForeignKey<Invoice>(x => x.VisitId);
            entity.HasMany(x => x.Lines).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Payments).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.DiscountApprovals).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.ToTable("InvoiceLines", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("InvoiceLineId");
            entity.Property(x => x.Quantity).HasPrecision(10, 2);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("PaymentId");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<DiscountApproval>(entity =>
        {
            entity.ToTable("DiscountApprovals", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("DiscountApprovalId");
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        });
    }

    private static void ConfigureCharity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CharityCase>(entity =>
        {
            entity.ToTable("CharityCases", "charity");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("CharityCaseId");
            entity.Property(x => x.EligibilityStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.CoveragePercentage).HasPrecision(5, 2);
            entity.HasIndex(x => x.CaseNumber).IsUnique();
            entity.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
            entity.HasMany(x => x.Invoices).WithOne(x => x.CharityCase).HasForeignKey(x => x.CharityCaseId);
        });

        modelBuilder.Entity<CharityCaseInvoice>(entity =>
        {
            entity.ToTable("CharityCaseInvoices", "charity");
            entity.HasKey(x => new { x.CharityCaseId, x.InvoiceId });
            entity.Property(x => x.CoveredAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.ToTable("Medicines", "inventory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("MedicineId");
            entity.Property(x => x.MedicineCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.GenericName).HasMaxLength(250).IsRequired();
            entity.HasIndex(x => x.MedicineCode).IsUnique();
            entity.HasMany(x => x.StockBatches).WithOne(x => x.Medicine).HasForeignKey(x => x.MedicineId);
            entity.HasMany(x => x.StockMovements).WithOne(x => x.Medicine).HasForeignKey(x => x.MedicineId);
        });

        modelBuilder.Entity<StockBatch>(entity =>
        {
            entity.ToTable("StockBatches", "inventory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("StockBatchId");
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 2);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.MedicineId, x.BatchNumber }).IsUnique();
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements", "inventory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("StockMovementId");
            entity.Property(x => x.MovementType).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
            entity.HasOne(x => x.StockBatch).WithMany().HasForeignKey(x => x.StockBatchId);
            entity.HasOne(x => x.PrescriptionItem).WithOne().HasForeignKey<StockMovement>(x => x.PrescriptionItemId);
        });
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("UserAccounts", "security");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("UserAccountId");
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasOne(x => x.StaffMember).WithOne().HasForeignKey<UserAccount>(x => x.StaffMemberId);
            entity.HasMany(x => x.UserRoles).WithOne(x => x.UserAccount).HasForeignKey(x => x.UserAccountId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles", "security");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("RoleId");
            entity.Property(x => x.RoleName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.RoleName).IsUnique();
            entity.HasMany(x => x.RolePermissions).WithOne(x => x.Role).HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions", "security");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("PermissionId");
            entity.Property(x => x.PermissionKey).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.PermissionKey).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles", "security");
            entity.HasKey(x => new { x.UserAccountId, x.RoleId });
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions", "security");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "audit");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("AuditLogId");
            entity.Property(x => x.ActionName).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(x => new { x.EntityName, x.EntityId });
            entity.HasIndex(x => new { x.UserAccountId, x.OccurredAt });
        });
    }
}
