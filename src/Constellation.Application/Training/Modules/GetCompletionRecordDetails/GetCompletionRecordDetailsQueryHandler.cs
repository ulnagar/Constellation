namespace Constellation.Application.Training.Modules.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Faculty.Identifiers;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCompletionRecordDetailsQueryHandler
    : IQueryHandler<GetCompletionRecordDetailsQuery, CompletionRecordDto>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetCompletionRecordDetailsQueryHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<GetCompletionRecordDetailsQuery>();
    }

    public async Task<Result<CompletionRecordDto>> Handle(GetCompletionRecordDetailsQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.ModuleId);

            return Result.Failure<CompletionRecordDto>(TrainingErrors.Module.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger.Warning("Could not find Training Completion with Id {completionId} in Module {moduleId}", request.CompletionId, request.ModuleId);

            return Result.Failure<CompletionRecordDto>(TrainingErrors.Completion.NotFound(request.CompletionId));
        }

        Staff staff = await _staffRepository.GetById(record.StaffId, cancellationToken);

        if (staff is null)
        {
            _logger.Warning("Could not find staff member with Id {id}", record.StaffId);

            return Result.Failure<CompletionRecordDto>(DomainErrors.Partners.Staff.NotFound(record.StaffId));
        }

        List<FacultyId> facultyIds = staff
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
            CreatedAt = record.CreatedAt
        };

        entity.ExpiryCountdown = entity.CalculateExpiry(_dateTime);
        entity.Status = CompletionRecordDto.ExpiryStatus.Active;

        return entity;
    }
}
