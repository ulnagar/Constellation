namespace Constellation.Application.Domains.Training.Queries.GetListOfCompletionRecords;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCompletionRecordsQueryHandler
    : IQueryHandler<GetListOfCompletionRecordsQuery, List<CompletionRecordDto>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IDateTimeProvider _dateTime;

    public GetListOfCompletionRecordsQueryHandler(
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ITrainingModuleRepository trainingRepository,
        IDateTimeProvider dateTime)
    {
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _trainingRepository = trainingRepository;
        _dateTime = dateTime;
    }

    public async Task<Result<List<CompletionRecordDto>>> Handle(GetListOfCompletionRecordsQuery request, CancellationToken cancellationToken)
    {
        List<CompletionRecordDto> data = new();
        List<StaffMember> staffMembers = new();

        if (request.StaffId == StaffId.Empty)
            staffMembers = await _staffRepository.GetAllActive(cancellationToken);
        else
            staffMembers.Add(await _staffRepository.GetById(request.StaffId, cancellationToken));

        List<TrainingModule> modules = await _trainingRepository.GetAllModules(cancellationToken);

        foreach (StaffMember staffMember in staffMembers)
        {
            List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staffMember.Id, cancellationToken);

            foreach (TrainingModule module in modules)
            {
                if (module.IsDeleted) continue;

                TrainingCompletion record = module.Completions
                    .Where(record =>
                        record.StaffId == staffMember.Id &&
                        !record.IsDeleted)
                    .MaxBy(record => record.CompletedDate);

                if (record is null)
                    continue;

                CompletionRecordDto entry = new()
                {
                    Id = record.Id,
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    ModuleExpiry = module.Expiry,
                    StaffId = record.StaffId,
                    StaffName = staffMember.Name,
                    StaffFirstName = staffMember.Name.FirstName,
                    StaffLastName = staffMember.Name.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = record.CompletedDate,
                    CreatedAt = record.CreatedAt,
                    Mandatory = module.Assignees.Any(entry => entry.StaffId == staffMember.Id)
                };

                entry.ExpiryCountdown = entry.CalculateExpiry(_dateTime);
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;

                data.Add(entry);
            }
        }

        return data;
    }
}
