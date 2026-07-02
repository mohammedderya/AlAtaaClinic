using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Invoices;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<InvoiceDto> UpdateAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<InvoiceDto> AddPaymentAsync(AddPaymentCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<InvoiceDto> GetByIdAsync(GetInvoiceByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<InvoiceListDto>> SearchAsync(SearchInvoicesQuery query, CancellationToken cancellationToken = default);
}
