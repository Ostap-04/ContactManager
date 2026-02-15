using ContactManager.Common.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace ContactManager;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddDataAccess(builder.Configuration);
        builder.Services.RegisterServices();

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
            .SetApplicationName("ContactManager");

        var app = builder.Build();

        await app.ApplyMigrationsAsync();


        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Contacts}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}