namespace ContactManager.Models.Entities;

public class Contact
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public bool Married { get; set; }

    public string Phone { get; set; } = null!;

    public decimal Salary { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}