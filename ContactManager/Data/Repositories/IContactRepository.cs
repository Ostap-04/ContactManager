using ContactManager.Models.Entities;

namespace ContactManager.Data.Repositories;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    IQueryable<Contact> Query();

    Task AddRangeAsync(IEnumerable<Contact> contacts, CancellationToken ct = default);

    void Update(Contact contact);

    void Remove(Contact contact);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}