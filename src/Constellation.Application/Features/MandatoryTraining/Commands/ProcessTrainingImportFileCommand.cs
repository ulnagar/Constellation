namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record ProcessTrainingImportFileCommand(MemoryStream stream) : IRequest { }

public class ProcessTrainingImportFileCommandHandler : IRequestHandler<ProcessTrainingImportFileCommand>
{
    private readonly IAppDbContext _context;
    private readonly IExcelService _excelService;

    public ProcessTrainingImportFileCommandHandler(IAppDbContext context, IExcelService excelService)
    {
        _context = context;
        _excelService = excelService;
    }

    public async Task<Unit> Handle(ProcessTrainingImportFileCommand request, CancellationToken cancellationToken)
    {
        var modules = _excelService.ImportMandatoryTrainingDataFromFile(request.stream);

        if (modules is null || !modules.Any())
            return Unit.Value;

        foreach (var module in modules)
        {
            // check if it exists in the db
            var existing = await _context.MandatoryTraining.Modules
                .Include(entry => entry.Completions)
                .Where(entry => entry.Name == module.Name)
                .FirstOrDefaultAsync(cancellationToken);

            var staff = await _context.Staff
                .Where(entry => !entry.IsDeleted)
                .Select(entry => entry.StaffId)
                .ToListAsync(cancellationToken);

            if (existing is not null)
            {
                foreach (var record in module.Completions)
                {
                    // Confirm that staff member exists and is current
                    if (!staff.Contains(record.StaffId))
                        continue;

                    if (existing.Completions.Any(entry => entry.StaffId == record.StaffId && entry.CompletedDate == record.CompletedDate))
                        continue;

                    record.TrainingModuleId = existing.Id;
                    record.Module = existing;

                    _context.Add(record);
                }

                continue;
            } else
            {
                module.Completions.RemoveAll(record => !staff.Contains(record.StaffId));

                _context.Add(module);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
