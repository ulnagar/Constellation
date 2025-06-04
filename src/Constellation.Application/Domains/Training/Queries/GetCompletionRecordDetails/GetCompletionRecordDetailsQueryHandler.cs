namespace Constellation.Application.Domains.Training.Queries.GetCompletionRecordDetails;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Models;
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
            _logger
                .ForContext(nameof(GetCompletionRecordDetailsQuery), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.ModuleId), true)
                .Warning("Could not find Training Module with Id {id}", request.ModuleId);

            return Result.Failure<CompletionRecordDto>(TrainingModuleErrors.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger
                .ForContext(nameof(GetCompletionRecordDetailsQuery), request, true)
                .ForContext(nameof(Error), TrainingCompletionErrors.NotFound(request.CompletionId), true)
                .Warning("Could not find Training Completion with Id {completionId} in Module {moduleId}", request.CompletionId, request.ModuleId);

            return Result.Failure<CompletionRecordDto>(TrainingCompletionErrors.NotFound(request.CompletionId));
        }

        StaffMember staff = await _staffRepository.GetById(record.StaffId, cancellationToken);

        if (staff is null)
        {
            _logger
                .ForContext(nameof(GetCompletionRecordDetailsQuery), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(record.StaffId), true)
                .Warning("Could not find staff member with Id {id}", record.StaffId);

            return Result.Failure<CompletionRecordDto>(StaffMemberErrors.NotFound(record.StaffId));
        }

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staff.Id, cancellationToken);

        CompletionRecordDto entity = new()
        {
            Id = record.Id,
            ModuleId = record.TrainingModuleId,
            ModuleName = module.Name,
            ModuleExpiry = module.Expiry,
            StaffId = record.StaffId,
            StaffFirstName = staff.Name.FirstName,
            StaffLastName = staff.Name.LastName,
            StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name).ToList()),
            CompletedDate = record.CompletedDate,
            CreatedAt = record.CreatedAt
        };

        entity.ExpiryCountdown = entity.CalculateExpiry(_dateTime);
        entity.Status = CompletionRecordDto.ExpiryStatus.Active;

        return entity;
    }
}
