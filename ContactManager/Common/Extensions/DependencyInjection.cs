using ContactManager.Data;
using ContactManager.Data.Repositories;
using ContactManager.Services.Implementations;
using ContactManager.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Common.Extensions;

public static class DependencyInjection
{
    public static void AddDataAccess(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
    }

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ICsvImporter, CsvImporter>();
        services.AddScoped<IContactService, ContactService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    }
}