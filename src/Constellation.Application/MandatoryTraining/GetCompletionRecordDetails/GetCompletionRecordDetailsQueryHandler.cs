namespace Constellation.Application.MandatoryTraining.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCompletionRecordDetailsQueryHandler 
    : IQueryHandler<GetCompletionRecordDetailsQuery, CompletionRecordDto>
{
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;

    public GetCompletionRecordDetailsQueryHandler(
        ITrainingCompletionRepository trainingCompletionRepository,
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository)
    {
        _trainingCompletionRepository = trainingCompletionRepository;
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<CompletionRecordDto>> Handle(GetCompletionRecordDetailsQuery request, CancellationToken cancellationToken)
    {
        var record = await _trainingCompletionRepository.GetById(request.Id, cancellationToken);

        var module = await _trainingModuleRepository.GetById(record.TrainingModuleId, cancellationToken);

        var staff = await _staffRepository.GetByIdWithFacultyMemberships(record.StaffId, cancellationToken);

        var facultyIds = staff
            .Faculties
            .Where(member => !member.IsDeleted)
            .Select(member => member.FacultyId)
            .ToList();

        var faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

        var entity = new CompletionRecordDto
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
