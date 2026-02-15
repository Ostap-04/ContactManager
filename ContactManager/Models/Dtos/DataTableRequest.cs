namespace ContactManager.Models.Dtos;

public class DataTableRequest
{
    public int Draw { get; init; }
    public int Start { get; init; }
    public int Length { get; init; }

    public string? SearchValue { get; init; }

    public List<DataTableOrder> Order { get; init; } = [];
    public List<DataTableColumn> Columns { get; init; } = [];
}

public class DataTableOrder
{
    public int Column { get; init; }
    public string Direction { get; init; } = "asc";
}

public class DataTableColumn
{
    public string? Data { get; init; } = string.Empty;
    public string? Name { get; init; } = string.Empty;
}