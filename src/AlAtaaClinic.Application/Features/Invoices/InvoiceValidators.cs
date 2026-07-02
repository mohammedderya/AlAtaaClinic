using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Invoices;

public sealed class CreateInvoiceCommandValidator : ValidatorBase<CreateInvoiceCommand>
{
    protected override void ValidateRules(CreateInvoiceCommand instance)
    {
        if (instance.BranchId <= 0) Add(nameof(instance.BranchId), "Branch is required.");
        if (instance.PatientId <= 0) Add(nameof(instance.PatientId), "Patient is required.");
        Required(instance.InvoiceNumber, nameof(instance.InvoiceNumber));
        NotNegative(instance.DiscountAmount, nameof(instance.DiscountAmount));
        ValidateLines(instance.Lines);
    }

    private void ValidateLines(IReadOnlyList<InvoiceLineDto> lines)
    {
        if (lines.Count == 0) Add(nameof(CreateInvoiceCommand.Lines), "At least one invoice line is required.");
        foreach (var line in lines) ValidateLine(line);
    }

    private void ValidateLine(InvoiceLineDto line)
    {
        Required(line.Description, nameof(InvoiceLineDto.Description));
        Positive(line.Quantity, nameof(InvoiceLineDto.Quantity));
        NotNegative(line.UnitPrice, nameof(InvoiceLineDto.UnitPrice));
        NotNegative(line.LineTotal, nameof(InvoiceLineDto.LineTotal));
    }
}

public sealed class UpdateInvoiceCommandValidator : ValidatorBase<UpdateInvoiceCommand>
{
    protected override void ValidateRules(UpdateInvoiceCommand instance)
    {
        if (instance.Id <= 0) Add(nameof(instance.Id), "Invoice id is required.");
        NotNegative(instance.DiscountAmount, nameof(instance.DiscountAmount));
        if (instance.Lines.Count == 0) Add(nameof(instance.Lines), "At least one invoice line is required.");
    }
}

public sealed class AddPaymentCommandValidator : ValidatorBase<AddPaymentCommand>
{
    protected override void ValidateRules(AddPaymentCommand instance)
    {
        if (instance.InvoiceId <= 0) Add(nameof(instance.InvoiceId), "Invoice is required.");
        Positive(instance.Amount, nameof(instance.Amount));
        MaxLength(instance.Notes, 500, nameof(instance.Notes));
    }
}
