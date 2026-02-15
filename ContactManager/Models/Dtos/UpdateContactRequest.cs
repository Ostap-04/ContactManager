namespace ContactManager.Models.Dtos;

public class UpdateContactRequest
{
    public string Name { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public bool Married { get; init; }
    public string Phone { get; init; } = string.Empty;
    public decimal Salary { get; init; }
}