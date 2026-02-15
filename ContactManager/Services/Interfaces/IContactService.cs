using ContactManager.Models.Dtos;
using ContactManager.Models.ServiceResults;

namespace ContactManager.Services.Interfaces;

public interface IContactService
{
    Task<DataTableResponse<ContactDto>>
        GetDataTableAsync(DataTableRequest request, CancellationToken cancellationToken = default);

    Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ImportContactsResult> ImportCsvAsync(Stream? stream,
        string fileName, CancellationToken cancellationToken = default);
}