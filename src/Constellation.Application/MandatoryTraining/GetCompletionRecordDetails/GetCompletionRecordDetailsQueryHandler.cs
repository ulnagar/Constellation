namespace Constellation.Application.MandatoryTraining.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCompletionRecordDetailsQueryHandler 
    : IQueryHandler<GetCompletionRecordDetailsQuery, CompletionRecordDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetCompletionRecordDetailsQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetCompletionRecordDetailsQuery>();
    }

    public async Task<Result<CompletionRecordDto>> Handle(GetCompletionRecordDetailsQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.ModuleId);

            return Result.Failure<CompletionRecordDto>(DomainErrors.MandatoryTraining.Module.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger.Warning("Could not find Training Completion with Id {completionId} in Module {moduleId}", request.CompletionId, request.ModuleId);

            return Result.Failure<CompletionRecordDto>(DomainErrors.MandatoryTraining.Completion.NotFound(request.CompletionId));
        }

        Staff staff = await _staffRepository.GetById(record.StaffId, cancellationToken);

        if (staff is null)
        {
            _logger.Warning("Could not find staff member with Id {id}", record.StaffId);

            return Result.Failure<CompletionRecordDto>(DomainErrors.Partners.Staff.NotFound(record.StaffId));
        }

        List<Guid> facultyIds = staff
            .Faculties
            .Where(member => !member.IsDeleted)
            .Select(member => member.FacultyId)
            .ToList();

        List<Faculty> faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

        CompletionRecordDto entity = new()
        {
            Id = record.Id,
            ModuleId = record.TrainingModuleId,
            ModuleName = module.Name,
            ModuleExpiry = module.Expiry,
            StaffId = record.StaffId,
            StaffFirstName = staff.FirstName,
            StaffLastName = staff.LastName,
            StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name).ToList()),
            CompletedDate = record.CompletedDate,
            NotRequired = record.NotRequired,
            CreatedAt = record.CreatedAt
        };

        entity.ExpiryCountdown = entity.CalculateExpiry();
        entity.Status = CompletionRecordDto.ExpiryStatus.Active;

        return entity;
    }
}
