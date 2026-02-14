using ContactManager.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Data.Repositories;

public class ContactRepository(AppDbContext context) : IContactRepository
{
    private readonly DbSet<Contact> _dbSet = context.Contacts;

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public IQueryable<Contact> Query()
        => _dbSet.AsNoTracking();

    public async Task AddRangeAsync(IEnumerable<Contact> contacts, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(contacts, cancellationToken);

    public void Update(Contact contact)
        => _dbSet.Update(contact);

    public void Remove(Contact contact)
        => _dbSet.Remove(contact);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}