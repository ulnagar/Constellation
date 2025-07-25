﻿namespace Constellation.Application.Domains.Training.Commands.ProcessTrainingImportFile;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessTrainingImportFileCommandHandler
    : ICommandHandler<ProcessTrainingImportFileCommand>
{
    private readonly IExcelService _excelService;
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessTrainingImportFileCommandHandler(
        IExcelService excelService,
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork)
    {
        _excelService = excelService;
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProcessTrainingImportFileCommand request, CancellationToken cancellationToken)
    {
        List<TrainingModule> modules = _excelService.ImportMandatoryTrainingDataFromFile(request.Stream);

        if (modules is null || modules.Count == 0)
            return Result.Failure(TrainingImportErrors.NoDataFound);

        foreach (TrainingModule module in modules)
        {
            // check if it exists in the db
            TrainingModule existing = await _trainingRepository.GetModuleByName(module.Name, cancellationToken);

            List<StaffId> staff = await _staffRepository.GetAllActiveStaffIds(cancellationToken);

            if (existing is not null)
            {
                foreach (TrainingCompletion record in module.Completions)
                {
                    // If the staff member is not valid or not current, skip the record
                    if (!staff.Contains(record.StaffId))
                        continue;

                    // If there is an existing record for this staff member and this completion date, skip the record
                    if (existing.Completions.Any(entry => entry.StaffId == record.StaffId && entry.CompletedDate == record.CompletedDate))
                        continue;

                    // Link the record to the database copy of the module
                    TrainingCompletion newRecord = TrainingCompletion.Create(
                        record.StaffId,
                        existing.Id,
                        record.CompletedDate);

                    module.AddCompletion(newRecord);
                }
            }
            else
            {
                foreach (TrainingCompletion record in module.Completions)
                {
                    if (!staff.Contains(record.StaffId))
                        module.RemoveCompletion(record);
                }

                _trainingRepository.Insert(module);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
