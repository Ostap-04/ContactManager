using ContactManager.Data.Repositories;
using ContactManager.Models.Dtos;
using ContactManager.Models.Entities;
using ContactManager.Models.ServiceResults;
using ContactManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Services.Implementations;

public sealed class ContactService(IContactRepository repository, ICsvImporter csvImporter) : IContactService
{
    public async Task<DataTableResponse<ContactDto>> GetDataTableAsync(DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = repository.Query().AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var search = request.SearchValue?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Phone.Contains(search));
        }

        var filtered = await query.CountAsync(cancellationToken);

        query = ApplyOrdering(query, request);

        var take = request.Length <= 0 ? 10 : request.Length;

        var items = await query
            .Skip(request.Start)
            .Take(take)
            .Select(x => new ContactDto(x.Id, x.Name, x.DateOfBirth, x.Married, x.Phone, x.Salary))
            .ToListAsync(cancellationToken);

        return new DataTableResponse<ContactDto>
        {
            Draw = request.Draw,
            RecordsTotal = total,
            RecordsFiltered = filtered,
            Data = items
        };
    }

    private static IQueryable<Contact> ApplyOrdering(IQueryable<Contact> query, DataTableRequest request)
    {
        var order = request.Order.FirstOrDefault();
        if (order is null)
        {
            return query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id);
        }

        var directionDesc = string.Equals(order.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        var column = (order.Column >= 0 && order.Column < request.Columns.Count)
            ? request.Columns[order.Column].Data
            : null;

        return (column?.ToLowerInvariant()) switch
        {
            "name" => directionDesc
                ? query.OrderByDescending(x => x.Name).ThenBy(x => x.Id)
                : query.OrderBy(x => x.Name).ThenBy(x => x.Id),

            "dateofbirth" => directionDesc
                ? query.OrderByDescending(x => x.DateOfBirth).ThenBy(x => x.Id)
                : query.OrderBy(x => x.DateOfBirth).ThenBy(x => x.Id),

            "married" => directionDesc
                ? query.OrderByDescending(x => x.Married).ThenBy(x => x.Id)
                : query.OrderBy(x => x.Married).ThenBy(x => x.Id),

            "phone" => directionDesc
                ? query.OrderByDescending(x => x.Phone).ThenBy(x => x.Id)
                : query.OrderBy(x => x.Phone).ThenBy(x => x.Id),

            "salary" => directionDesc
                ? query.OrderByDescending(x => x.Salary).ThenBy(x => x.Id)
                : query.OrderBy(x => x.Salary).ThenBy(x => x.Id),

            _ => query.OrderByDescending(x => x.CreatedAtUtc).ThenBy(x => x.Id)
        };
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await repository.Query().AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ContactDto(x.Id, x.Name, x.DateOfBirth, x.Married, x.Phone, x.Salary))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.Name = request.Name?.Trim()!;
        entity.Phone = request.Phone?.Trim()!;
        entity.DateOfBirth = request.DateOfBirth;
        entity.Married = request.Married;
        entity.Salary = request.Salary;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        repository.Remove(entity);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ImportContactsResult> ImportCsvAsync(Stream? stream,
        string fileName, CancellationToken cancellationToken = default)
    {
        if (stream is null or { CanSeek: true, Length: 0 })
            return new ImportContactsResult(0, 0, ["File is empty."]);

        if (string.IsNullOrWhiteSpace(fileName) ||
            !Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return new ImportContactsResult(0, 0, ["Only .csv files are supported."]);
        }

        if (stream.CanSeek)
            stream.Position = 0;

        var result = await csvImporter.ImportContactsAsync(stream, cancellationToken);

        if (result.Contacts.Count > 0)
        {
            await repository.AddRangeAsync(result.Contacts, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return new ImportContactsResult(
            result.Contacts.Count,
            result.Errors.Count,
            result.Errors);
    }
}