﻿namespace Constellation.Application.Domains.Training.Queries.GetListOfCompletionRecords;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.Repositories;
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
        List<Staff> staffMembers = new();

        if (string.IsNullOrWhiteSpace(request.StaffId))
            staffMembers = await _staffRepository.GetAllActive(cancellationToken);
        else
            staffMembers.Add(await _staffRepository.GetById(request.StaffId, cancellationToken));

        List<TrainingModule> modules = await _trainingRepository.GetAllModules(cancellationToken);

        foreach (Staff staffMember in staffMembers)
        {
            List<FacultyId> facultyIds = staffMember
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            List<Faculty> faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            foreach (TrainingModule module in modules)
            {
                if (module.IsDeleted) continue;

                TrainingCompletion record = module.Completions
                    .Where(record =>
                        record.StaffId == staffMember.StaffId &&
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
                    StaffName = staffMember.GetName(),
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = record.CompletedDate,
                    CreatedAt = record.CreatedAt,
                    Mandatory = module.Assignees.Any(entry => entry.StaffId == staffMember.StaffId)
                };

                entry.ExpiryCountdown = entry.CalculateExpiry(_dateTime);
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;

                data.Add(entry);
            }
        }

        return data;
    }
}
