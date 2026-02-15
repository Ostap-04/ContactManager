namespace ContactManager.Models.Dtos;

public class DataTableResponse<T>
{
    public required int Draw { get; init; }
    public required int RecordsTotal { get; init; }
    public required int RecordsFiltered { get; init; }
    public required IReadOnlyList<T> Data { get; init; }
}