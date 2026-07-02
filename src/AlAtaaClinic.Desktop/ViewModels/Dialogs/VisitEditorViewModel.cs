using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Doctors;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Application.Features.Visits;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class VisitEditorViewModel : DialogViewModelBase
{
    private readonly IVisitService _visitService;
    private readonly VisitDto? _existingVisit;

    public VisitEditorViewModel(IVisitService visitService, IExceptionHandler exceptionHandler, VisitDto? visit = null)
        : base(exceptionHandler)
    {
        _visitService = visitService;
        _existingVisit = visit;
        Diagnoses = [];
        VitalSigns = [];
        Procedures = [];
        Prescriptions = [];
        Load(visit);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public VisitStatus[] Statuses { get; } = Enum.GetValues<VisitStatus>();
    public VisitStatus Status { get; set; } = VisitStatus.Open;
    public string? ChiefComplaint { get; set; }
    public string? ClinicalNotes { get; set; }

    public ObservableCollection<BranchListDto> Branches { get; } = [];
    public BranchListDto? SelectedBranch { get; set; }
    public long BranchId => SelectedBranch?.Id ?? 0;

    public ObservableCollection<PatientListDto> Patients { get; } = [];
    public PatientListDto? SelectedPatient { get; set; }
    public long PatientId => SelectedPatient?.Id ?? 0;

    public ObservableCollection<DoctorListDto> Doctors { get; } = [];
    public DoctorListDto? SelectedDoctor { get; set; }
    public long DoctorId => SelectedDoctor?.Id ?? 0;

    public long? AppointmentId { get; set; }

    public void SetLookups(
        IReadOnlyList<BranchListDto> branches,
        IReadOnlyList<PatientListDto> patients,
        IReadOnlyList<DoctorListDto> doctors)
    {
        Branches.Clear();
        foreach (var b in branches) Branches.Add(b);

        Patients.Clear();
        foreach (var p in patients) Patients.Add(p);

        Doctors.Clear();
        foreach (var d in doctors) Doctors.Add(d);

        if (Id > 0)
        {
            SelectedBranch = Branches.FirstOrDefault(b => b.Id == _originalBranchId);
            SelectedPatient = Patients.FirstOrDefault(p => p.Id == _originalPatientId);
            SelectedDoctor = Doctors.FirstOrDefault(d => d.Id == _originalDoctorId);
        }
    }

    private long _originalBranchId;
    private long _originalPatientId;
    private long _originalDoctorId;

    public void SelectPatient(long patientId)
    {
        SelectedPatient = Patients.FirstOrDefault(p => p.Id == patientId);
    }

    public ObservableCollection<DiagnosisEntry> Diagnoses { get; }
    public ObservableCollection<VitalSignEntry> VitalSigns { get; }
    public ObservableCollection<ProcedureEntry> Procedures { get; }
    public ObservableCollection<PrescriptionEntry> Prescriptions { get; }

    public ICommand AddDiagnosisCommand => _addDiagnosis ??= new RelayCommand(_ => Diagnoses.Add(new DiagnosisEntry()));
    private RelayCommand? _addDiagnosis;

    public ICommand AddVitalSignCommand => _addVitalSign ??= new RelayCommand(_ => VitalSigns.Add(new VitalSignEntry()));
    private RelayCommand? _addVitalSign;

    public ICommand AddProcedureCommand => _addProcedure ??= new RelayCommand(_ => Procedures.Add(new ProcedureEntry()));
    private RelayCommand? _addProcedure;

    public ICommand AddPrescriptionCommand => _addPrescription ??= new RelayCommand(_ => Prescriptions.Add(new PrescriptionEntry()));
    private RelayCommand? _addPrescription;

    public ICommand RemoveDiagnosisCommand => _removeDiagnosis ??= new RelayCommand(item => Diagnoses.Remove((DiagnosisEntry)item!));
    private RelayCommand? _removeDiagnosis;
    public ICommand RemoveVitalSignCommand => _removeVitalSign ??= new RelayCommand(item => VitalSigns.Remove((VitalSignEntry)item!));
    private RelayCommand? _removeVitalSign;
    public ICommand RemoveProcedureCommand => _removeProcedure ??= new RelayCommand(item => Procedures.Remove((ProcedureEntry)item!));
    private RelayCommand? _removeProcedure;

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            if (BranchId == 0) { ValidationMessages.Add("الرجاء اختيار الفرع."); return; }
            if (PatientId == 0) { ValidationMessages.Add("الرجاء اختيار المريض."); return; }
            if (DoctorId == 0) { ValidationMessages.Add("الرجاء اختيار الطبيب."); return; }

            if (IsEditMode)
            {
                await _visitService.UpdateAsync(ToUpdateCommand());
                Close(true);
                return;
            }

            await _visitService.CreateAsync(ToCreateCommand());
            Close(true);
        });
    }

    private CreateVisitCommand ToCreateCommand()
    {
        foreach (var p in Prescriptions) p.DoctorId = DoctorId;
        return new CreateVisitCommand(
            BranchId, PatientId, DoctorId, AppointmentId,
            ChiefComplaint, ClinicalNotes,
            VitalSigns.Select(v => v.ToDto()).ToList(),
            Diagnoses.Select(d => d.ToDto()).ToList(),
            Procedures.Select(p => p.ToDto()).ToList(),
            Prescriptions.Select(p => p.ToDto()).ToList(),
            []);
    }

    private UpdateVisitCommand ToUpdateCommand()
    {
        foreach (var p in Prescriptions) p.DoctorId = DoctorId;
        return new UpdateVisitCommand(
            Id, Status, ChiefComplaint, ClinicalNotes,
            Status == VisitStatus.Closed ? DateTime.UtcNow : null,
            VitalSigns.Select(v => v.ToDto()).ToList(),
            Diagnoses.Select(d => d.ToDto()).ToList(),
            Procedures.Select(p => p.ToDto()).ToList(),
            Prescriptions.Select(p => p.ToDto()).ToList(),
            _existingVisit?.MedicalOrders ?? []);
    }

    private void Load(VisitDto? visit)
    {
        if (visit is null) return;
        Id = visit.Id;
        _originalBranchId = visit.BranchId;
        _originalPatientId = visit.PatientId;
        _originalDoctorId = visit.DoctorId;
        AppointmentId = visit.AppointmentId;
        Status = visit.Status;
        ChiefComplaint = visit.ChiefComplaint;
        ClinicalNotes = visit.ClinicalNotes;

        foreach (var d in visit.Diagnoses)
            Diagnoses.Add(new DiagnosisEntry(d.DiagnosisCode, d.Description, d.IsPrimary));
        foreach (var v in visit.VitalSigns)
            VitalSigns.Add(new VitalSignEntry(v.SystolicPressure, v.DiastolicPressure, v.Temperature, v.WeightKg, v.HeightCm, v.Pulse));
        foreach (var p in visit.Procedures)
            Procedures.Add(new ProcedureEntry(p.ProcedureName, p.Notes));
        foreach (var p in visit.Prescriptions)
            Prescriptions.Add(new PrescriptionEntry(p.DoctorId, p.Notes, p.Items));
    }
}

public sealed class DiagnosisEntry : ViewModelBase
{
    public DiagnosisEntry() { }
    public DiagnosisEntry(string? code, string description, bool isPrimary)
    {
        _diagnosisCode = code;
        _description = description;
        _isPrimary = isPrimary;
    }

    private string? _diagnosisCode;
    private string _description = string.Empty;
    private bool _isPrimary;

    public string? DiagnosisCode { get => _diagnosisCode; set => SetProperty(ref _diagnosisCode, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public bool IsPrimary { get => _isPrimary; set => SetProperty(ref _isPrimary, value); }

    public DiagnosisDto ToDto() => new(DiagnosisCode, Description, IsPrimary);
}

public sealed class VitalSignEntry : ViewModelBase
{
    public VitalSignEntry() { }
    public VitalSignEntry(int? systolic, int? diastolic, decimal? temp, decimal? weight, decimal? height, int? pulse)
    {
        _systolicPressure = systolic;
        _diastolicPressure = diastolic;
        _temperature = temp;
        _weightKg = weight;
        _heightCm = height;
        _pulse = pulse;
    }

    private int? _systolicPressure;
    private int? _diastolicPressure;
    private decimal? _temperature;
    private decimal? _weightKg;
    private decimal? _heightCm;
    private int? _pulse;

    public int? SystolicPressure { get => _systolicPressure; set => SetProperty(ref _systolicPressure, value); }
    public int? DiastolicPressure { get => _diastolicPressure; set => SetProperty(ref _diastolicPressure, value); }
    public decimal? Temperature { get => _temperature; set => SetProperty(ref _temperature, value); }
    public decimal? WeightKg { get => _weightKg; set => SetProperty(ref _weightKg, value); }
    public decimal? HeightCm { get => _heightCm; set => SetProperty(ref _heightCm, value); }
    public int? Pulse { get => _pulse; set => SetProperty(ref _pulse, value); }

    public VitalSignDto ToDto() => new(SystolicPressure, DiastolicPressure, Temperature, WeightKg, HeightCm, Pulse);
}

public sealed class ProcedureEntry : ViewModelBase
{
    public ProcedureEntry() { }
    public ProcedureEntry(string name, string? notes) { _procedureName = name; _notes = notes; }

    private string _procedureName = string.Empty;
    private string? _notes;

    public string ProcedureName { get => _procedureName; set => SetProperty(ref _procedureName, value); }
    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

    public ClinicalProcedureDto ToDto() => new(ProcedureName, Notes);
}

public sealed class PrescriptionEntry : ViewModelBase
{
    public PrescriptionEntry() { }
    public PrescriptionEntry(long doctorId, string? notes, IReadOnlyList<PrescriptionItemDto> items)
    {
        _doctorId = doctorId;
        _notes = notes;
        foreach (var item in items) Items.Add(new PrescriptionDrugItem(item));
    }

    private long _doctorId;
    private string? _notes;

    public long DoctorId { get => _doctorId; set => SetProperty(ref _doctorId, value); }
    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }
    public ObservableCollection<PrescriptionDrugItem> Items { get; } = [];

    public ICommand AddItemCommand => _addItem ??= new RelayCommand(_ => Items.Add(new PrescriptionDrugItem()));
    private RelayCommand? _addItem;

    public PrescriptionDto ToDto() => new(DoctorId, Notes, Items.Select(i => i.ToDto()).ToList());
}

public sealed class PrescriptionDrugItem : ViewModelBase
{
    public PrescriptionDrugItem() { }
    public PrescriptionDrugItem(PrescriptionItemDto dto)
    {
        _medicineName = dto.MedicineName;
        _dosage = dto.Dosage;
        _frequency = dto.Frequency;
        _duration = dto.Duration;
        _instructions = dto.Instructions;
        _quantity = dto.Quantity;
    }

    private string _medicineName = string.Empty;
    private string _dosage = string.Empty;
    private string _frequency = string.Empty;
    private string _duration = string.Empty;
    private string? _instructions;
    private decimal? _quantity;

    public string MedicineName { get => _medicineName; set => SetProperty(ref _medicineName, value); }
    public string Dosage { get => _dosage; set => SetProperty(ref _dosage, value); }
    public string Frequency { get => _frequency; set => SetProperty(ref _frequency, value); }
    public string Duration { get => _duration; set => SetProperty(ref _duration, value); }
    public string? Instructions { get => _instructions; set => SetProperty(ref _instructions, value); }
    public decimal? Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

    public PrescriptionItemDto ToDto() => new(MedicineName, Dosage, Frequency, Duration, Instructions, Quantity);
}
