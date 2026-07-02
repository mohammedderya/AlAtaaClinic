using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Invoices;

public sealed record GetInvoiceByIdQuery(long Id) : IQuery<InvoiceDto>;

public sealed record SearchInvoicesQuery(long? PatientId, string? Status, PageRequest Page) : IQuery<PagedResult<InvoiceListDto>>;
