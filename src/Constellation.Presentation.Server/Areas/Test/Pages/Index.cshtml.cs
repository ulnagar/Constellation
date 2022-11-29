namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelService _excelService;
    private readonly IAppDbContext _context;

    public IndexModel(IUnitOfWork unitOfWork, IExcelService excelService, IAppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _excelService = excelService;
        _context = context;
    }

    [BindProperty]
    public IFormFile UploadFile { get; set; }

    public async Task OnGetAsync()
    {
        await GetClasses(_unitOfWork);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (UploadFile is not null)
        {
            var modules = new List<TrainingModule>();

            try
            {
                await using var target = new MemoryStream();
                await UploadFile.CopyToAsync(target);

                modules = _excelService.ImportMandatoryTrainingDataFromFile(target);
            }
            catch (Exception ex)
            {
                // Error uploading file
                throw ex;
            }

            if (modules is null || modules.Count == 0)
                return Page();

            foreach (var module in modules)
            {
                // check if it exists in the db
                var existing = await _context.MandatoryTraining.Modules
                    .Include(entry => entry.Completions)
                    .Where(entry => entry.Name == module.Name)
                    .FirstOrDefaultAsync();

                if (existing is not null)
                {
                    foreach (var record in module.Completions)
                    {
                        if (existing.Completions.Any(entry => entry.StaffId == record.StaffId && entry.CompletedDate == record.CompletedDate))
                            continue;

                        record.TrainingModuleId = existing.Id;
                        record.Module = existing;

                        _context.Add(record);
                    }

                    continue;
                }

                _context.Add(module);
            }

            await _context.SaveChangesAsync(default);
        }

        return Page();
    }
}
