using ContactManager.Models.ServiceResults;

namespace ContactManager.Services.Interfaces;

public interface ICsvImporter
{
    Task<CsvImportResult> ImportContactsAsync(Stream csvStream, CancellationToken cancellationToken = default);
}