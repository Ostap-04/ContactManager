using ContactManager.Common.Constants;
using ContactManager.Models.Dtos;
using ContactManager.Models.ViewModels;
using ContactManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Controllers;

public sealed class ContactsController(IContactService service) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Data([FromForm] DataTableRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = Array.Empty<ContactDto>(),
                error = "Invalid request."
            });
        }

        var result = await service.GetDataTableAsync(request, cancellationToken);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var contactDto = await service.GetByIdAsync(id, cancellationToken);
        if (contactDto is null)
            return NotFound();

        var model = new UpdateContactRequest
        {
            Name = contactDto.Name,
            DateOfBirth = contactDto.DateOfBirth,
            Married = contactDto.Married,
            Phone = contactDto.Phone,
            Salary = contactDto.Salary
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateContactRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var success = await service.UpdateAsync(id, request, cancellationToken);
        if (success)
            return RedirectToAction(nameof(Index));

        ModelState.AddModelError(string.Empty, "Invalid data or contact not found.");
        return View(request);
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await service.DeleteAsync(id, cancellationToken);
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        const long maxFileSize = FileUploadDefaults.MaxCsvSizeBytes;

        if (file is null || file.Length == 0)
            return View("UploadResult", new UploadResultViewModel { Errors = ["File is empty."] });

        if (file.Length > maxFileSize)
            return View("UploadResult", new UploadResultViewModel
            {
                Errors = [$"File is too large. Maximum allowed size is {maxFileSize / (1024 * 1024)} MB."]
            });

        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return View("UploadResult", new UploadResultViewModel { Errors = ["Only .csv files are supported."] });

        await using var stream = file.OpenReadStream();

        var result = await service.ImportCsvAsync(stream, file.FileName, cancellationToken);

        return View("UploadResult", new UploadResultViewModel
        {
            Imported = result.Imported,
            Failed = result.Failed,
            Errors = result.Errors.ToList()
        });
    }
}