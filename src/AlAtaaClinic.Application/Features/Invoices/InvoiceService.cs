using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Billing;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Invoices;

public sealed class InvoiceService :
    IInvoiceService,
    ICommandHandler<CreateInvoiceCommand, InvoiceDto>,
    ICommandHandler<UpdateInvoiceCommand, InvoiceDto>,
    ICommandHandler<AddPaymentCommand, InvoiceDto>,
    ICommandHandler<DeleteInvoiceCommand, OperationResult>,
    IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>,
    IQueryHandler<SearchInvoicesQuery, PagedResult<InvoiceListDto>>
{
    private readonly IInvoiceRepository _invoices;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateInvoiceCommand> _createValidator;
    private readonly ValidationRunner<UpdateInvoiceCommand> _updateValidator;
    private readonly ValidationRunner<AddPaymentCommand> _paymentValidator;
    private readonly IAppLogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository invoices,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateInvoiceCommand> createValidator,
        ValidationRunner<UpdateInvoiceCommand> updateValidator,
        ValidationRunner<AddPaymentCommand> paymentValidator,
        IAppLogger<InvoiceService> logger)
    {
        _invoices = invoices;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _paymentValidator = paymentValidator;
        _logger = logger;
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.BillingWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureInvoiceNumberIsUniqueAsync(command.InvoiceNumber, cancellationToken);

        var invoice = CreateEntity(command);
        await _invoices.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Invoice '{invoice.InvoiceNumber}' created.");
        return invoice.ToDto();
    }

    public async Task<InvoiceDto> UpdateAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.BillingWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var invoice = await GetInvoiceOrThrowAsync(command.Id, cancellationToken);
        Apply(command, invoice);
        _invoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice.ToDto();
    }

    public async Task<InvoiceDto> AddPaymentAsync(AddPaymentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.BillingWrite, cancellationToken);
        _paymentValidator.ValidateAndThrow(command);

        var invoice = await GetInvoiceOrThrowAsync(command.InvoiceId, cancellationToken);
        invoice.Payments.Add(CreatePayment(command));
        UpdateStatus(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.BillingWrite, cancellationToken);
        var invoice = await GetInvoiceOrThrowAsync(command.Id, cancellationToken);
        invoice.Status = InvoiceStatus.Voided;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Invoice voided.");
    }

    public async Task<InvoiceDto> GetByIdAsync(GetInvoiceByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.BillingRead, cancellationToken);
        var invoice = await GetInvoiceOrThrowAsync(query.Id, cancellationToken);
        return invoice.ToDto();
    }

    public async Task<PagedResult<InvoiceListDto>> SearchAsync(SearchInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.BillingRead, cancellationToken);
        var result = await _invoices.SearchAsync(query.PatientId, query.Status, query.Page, cancellationToken);
        return result.Map(InvoiceMapper.ToListDto);
    }

    public Task<InvoiceDto> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default) => CreateAsync(command, cancellationToken);
    public Task<InvoiceDto> HandleAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default) => UpdateAsync(command, cancellationToken);
    public Task<InvoiceDto> HandleAsync(AddPaymentCommand command, CancellationToken cancellationToken = default) => AddPaymentAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(DeleteInvoiceCommand command, CancellationToken cancellationToken = default) => DeleteAsync(command, cancellationToken);
    public Task<InvoiceDto> HandleAsync(GetInvoiceByIdQuery query, CancellationToken cancellationToken = default) => GetByIdAsync(query, cancellationToken);
    public Task<PagedResult<InvoiceListDto>> HandleAsync(SearchInvoicesQuery query, CancellationToken cancellationToken = default) => SearchAsync(query, cancellationToken);

    private async Task<Invoice> GetInvoiceOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _invoices.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), id);
    }

    private async Task EnsureInvoiceNumberIsUniqueAsync(string invoiceNumber, CancellationToken cancellationToken)
    {
        if (await _invoices.AnyAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken))
        {
            throw new ConflictException($"Invoice number '{invoiceNumber}' already exists.");
        }
    }

    private static Invoice CreateEntity(CreateInvoiceCommand command)
    {
        var invoice = new Invoice
        {
            BranchId = command.BranchId,
            PatientId = command.PatientId,
            VisitId = command.VisitId,
            InvoiceNumber = command.InvoiceNumber.Trim(),
            DiscountAmount = command.DiscountAmount,
            Lines = command.Lines.Select(InvoiceMapper.ToEntity).ToList()
        };

        Recalculate(invoice);
        return invoice;
    }

    private static void Apply(UpdateInvoiceCommand command, Invoice invoice)
    {
        invoice.Status = command.Status;
        invoice.DiscountAmount = command.DiscountAmount;
        invoice.Lines.Clear();
        invoice.Lines.AddRange(command.Lines.Select(InvoiceMapper.ToEntity));
        invoice.UpdatedAt = DateTime.UtcNow;
        Recalculate(invoice);
    }

    private static Payment CreatePayment(AddPaymentCommand command)
    {
        return new Payment
        {
            Amount = command.Amount,
            PaymentMethod = command.PaymentMethod,
            PaidAt = DateTime.UtcNow,
            ReceivedByStaffMemberId = command.ReceivedByStaffMemberId,
            Notes = command.Notes?.Trim()
        };
    }

    private static void Recalculate(Invoice invoice)
    {
        invoice.GrossAmount = invoice.Lines.Sum(line => line.LineTotal);
        invoice.NetAmount = Math.Max(0, invoice.GrossAmount - invoice.DiscountAmount);
    }

    private static void UpdateStatus(Invoice invoice)
    {
        var paidAmount = invoice.Payments.Sum(payment => payment.Amount);
        invoice.Status = paidAmount >= invoice.NetAmount ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
    }
}
