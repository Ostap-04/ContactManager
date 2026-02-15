using System.Globalization;
using ContactManager.Models.Entities;
using ContactManager.Models.ServiceResults;
using ContactManager.Services.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace ContactManager.Services.Implementations;

public class CsvImporter : ICsvImporter
{
    private sealed class ContactCsvRow
    {
        public string Name { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Married { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
    }

    private sealed class ContactCsvMap : ClassMap<ContactCsvRow>
    {
        public ContactCsvMap()
        {
            Map(m => m.Name).Name("Name", "name");
            Map(m => m.DateOfBirth).Name("Date of birth", "DateOfBirth", "date of birth", "dob");
            Map(m => m.Married).Name("Married", "married");
            Map(m => m.Phone).Name("Phone", "phone");
            Map(m => m.Salary).Name("Salary", "salary");
        }
    }

    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "dd.MM.yyyy",
        "MM/dd/yyyy",
        "dd/MM/yyyy"
    ];

    public async Task<CsvImportResult> ImportContactsAsync(Stream csvStream, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var contacts = new List<Contact>();

        using var reader = new StreamReader(csvStream, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
            DetectDelimiter = true,
        };

        using var csvReader = new CsvReader(reader, config);
        csvReader.Context.RegisterClassMap<ContactCsvMap>();

        var rowNumber = 1; // used to show errors, so start from 1 (not 0)
        await foreach (var row in csvReader.GetRecordsAsync<ContactCsvRow>(cancellationToken))
        {
            rowNumber++;

            var success = TryConvert(row, out var contact, out var error);
            if (success)
            {
                contacts.Add(contact!);
            }
            else
            {
                errors.Add($"Row {rowNumber}: {error}");
            }
        }

        return new CsvImportResult
        (
            contacts,
            errors
        );
    }

    private static bool TryConvert(ContactCsvRow row, out Contact? contact, out string error)
    {
        contact = null;
        error = string.Empty;

        var name = row.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            error = "Invalid Name (required, max 200).";
            return false;
        }

        var phone = row.Phone?.Trim();
        if (string.IsNullOrWhiteSpace(phone) || phone.Length > 32)
        {
            error = "Invalid Phone (required, max 32).";
            return false;
        }

        if (!TryParseDateOnly(row.DateOfBirth, out var dateOfBirth))
        {
            error = "Invalid Date of birth (expected date).";
            return false;
        }

        if (!TryParseBool(row.Married, out var married))
        {
            error = "Invalid Married (expected true/false or 1/0 or yes/no).";
            return false;
        }

        if (!decimal.TryParse(row.Salary?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var salary))
        {
            if (!decimal.TryParse(row.Salary?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out salary))
            {
                error = "Invalid Salary (expected decimal).";
                return false;
            }
        }

        contact = new Contact
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            DateOfBirth = dateOfBirth,
            Married = married,
            Salary = salary,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null
        };

        return true;
    }

    private static bool TryParseBool(string? value, out bool result)
    {
        result = false;

        var trimmedValue = value?.Trim();

        if (string.IsNullOrEmpty(trimmedValue))
            return false;

        if (bool.TryParse(trimmedValue, out result))
            return true;

        switch (trimmedValue)
        {
            case "1" or "yes" or "y" or "true" or "t":
                result = true;
                return true;
            case "0" or "no" or "n" or "false" or "f":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    private static bool TryParseDateOnly(string? value, out DateOnly dateOfBirth)
    {
        dateOfBirth = default;

        var dateString = value?.Trim();

        if (string.IsNullOrEmpty(dateString))
            return false;

        if (DateOnly.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfBirth))
            return true;

        if (DateOnly.TryParseExact(dateString, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dateOfBirth))
            return true;

        return DateOnly.TryParse(dateString, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateOfBirth);
    }
}