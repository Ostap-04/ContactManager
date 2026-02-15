namespace ContactManager.Models.Dtos;

public record ContactDto(
    Guid Id,
    string Name,
    DateOnly DateOfBirth,
    bool Married,
    string Phone,
    decimal Salary);