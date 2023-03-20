namespace Constellation.Application.MandatoryTraining.ProcessTrainingImportFile;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessTrainingImportFileCommandHandler
    : ICommandHandler<ProcessTrainingImportFileCommand>
{
    private readonly IExcelService _excelService;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessTrainingImportFileCommandHandler(
        IExcelService excelService,
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        ITrainingCompletionRepository trainingCompletionRepository,
        IUnitOfWork unitOfWork)
    {
        _excelService = excelService;
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _trainingCompletionRepository = trainingCompletionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProcessTrainingImportFileCommand request, CancellationToken cancellationToken)
    {
        var modules = _excelService.ImportMandatoryTrainingDataFromFile(request.Stream);

        if (modules is null || !modules.Any())
            return Result.Failure(DomainErrors.MandatoryTraining.Import.NoDataFound);

        foreach (var module in modules)
        {
            // check if it exists in the db
            var existing = await _trainingModuleRepository.GetWithCompletionByName(module.Name, cancellationToken);

            var staff = await _staffRepository.GetAllActiveStaffIds(cancellationToken);

            if (existing is not null)
            {
                foreach (var record in module.Completions)
                {
                    // If the staff member is not valid or not current, skip the record
                    if (!staff.Contains(record.StaffId))
                        continue;

                    // If there is an existing record for this staff member and this completion date, skip the record
                    if (existing.Completions.Any(entry => entry.StaffId == record.StaffId && entry.CompletedDate == record.CompletedDate))
                        continue;

                    // Link the record to the database copy of the module
                    var newRecord = TrainingCompletion.Create(
                        new TrainingCompletionId(),
                        record.StaffId,
                        existing.Id);

                    newRecord.SetCompletedDate(record.CompletedDate.Value);

                    // Save the record to the database
                    _trainingCompletionRepository.Insert(newRecord);
                }

                continue;
            }
            else
            {
                foreach (var record in module.Completions)
                {
                    if (!staff.Contains(record.StaffId))
                        module.RemoveCompletion(record);
                }

                _trainingModuleRepository.Insert(module);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
