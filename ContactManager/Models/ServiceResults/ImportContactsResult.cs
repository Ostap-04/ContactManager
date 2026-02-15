namespace ContactManager.Models.ServiceResults;

public record ImportContactsResult(
    int Imported,
    int Failed,
    IReadOnlyList<string> Errors);