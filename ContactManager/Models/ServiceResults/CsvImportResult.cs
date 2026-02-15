using ContactManager.Models.Entities;

namespace ContactManager.Models.ServiceResults;

public sealed record CsvImportResult(
    IReadOnlyList<Contact> Contacts,
    IReadOnlyList<string> Errors);