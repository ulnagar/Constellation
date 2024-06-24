namespace Constellation.Application.Training.GetModuleDetails;

using Abstractions.Messaging;
using Constellation.Core.Models;
using Core.Abstractions.Clock;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Helpers;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetModuleDetailsQueryHandler
    : IQueryHandler<GetModuleDetailsQuery, ModuleDetailsDto>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IDateTimeProvider _dateTime;

    public GetModuleDetailsQueryHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IDateTimeProvider dateTime)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _dateTime = dateTime;
    }

    public async Task<Result<ModuleDetailsDto>> Handle(GetModuleDetailsQuery request, CancellationToken cancellationToken)
    {
        // Get info from database
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<CompletionRecordDto> completions = new();
        List<ModuleDetailsDto.Assignee> assignees = new();

        foreach (Staff staffMember in staffMembers)
        {
            if (module.Assignees.Any(entry => entry.StaffId == staffMember.StaffId))
            {
                assignees.Add(new(
                    staffMember.StaffId,
                    staffMember.GetName()));
            }

            List<FacultyId> facultyIds = staffMember
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            List<Faculty> faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            List<TrainingCompletion> records = module.Completions
                    .Where(record =>
                        record.StaffId == staffMember.StaffId &&
                        !record.IsDeleted)
                    .ToList();

            TrainingCompletion record = records.MaxBy(record => record.CompletedDate);
            
            if (record is null)
            {
                CompletionRecordDto entry = new()
                {
                    StaffId = staffMember.StaffId,
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = null,
                    ExpiryCountdown = -9999
                };

                completions.Add(entry);
            }
            else
            {
                CompletionRecordDto entry = new()
                {
                    Id = record.Id,
                    ModuleId = record.TrainingModuleId,
                    ModuleName = module.Name,
                    ModuleExpiry = module.Expiry,
                    StaffId = record.StaffId,
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = record.CompletedDate,
                    CreatedAt = record.CreatedAt
                };

                entry.ExpiryCountdown = entry.CalculateExpiry(_dateTime);
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;

                completions.Add(entry);
            }
        }

        ModuleDetailsDto data = new(
            module.Id,
            module.Name,
            module.Expiry.GetDisplayName(),
            string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url,
            completions,
            !module.IsDeleted,
            assignees);

        return data;
    }
}
