﻿namespace Constellation.Application.Training.Modules.GetModuleDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Faculty.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training.Repositories;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetModuleDetailsQueryHandler
    : IQueryHandler<GetModuleDetailsQuery, ModuleDetailsDto>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IDateTimeProvider _dateTime;

    public GetModuleDetailsQueryHandler(
        ITrainingRoleRepository roleRepository,
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IDateTimeProvider dateTime)
    {
        _roleRepository = roleRepository;
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _dateTime = dateTime;
    }

    public async Task<Result<ModuleDetailsDto>> Handle(GetModuleDetailsQuery request, CancellationToken cancellationToken)
    {
        ModuleDetailsDto data = new();

        // Get info from database
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        data.Id = module.Id;
        data.Name = module.Name;
        data.Expiry = module.Expiry.GetDisplayName();
        data.Url = string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url;
        data.IsActive = !module.IsDeleted;

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<TrainingRole> roles = await _roleRepository.GetRolesForModule(module.Id, cancellationToken);
        List<string> requiredStaffIds = roles
            .SelectMany(role => role.Members)
            .Select(member => member.StaffId)
            .Distinct()
            .ToList();

        foreach (Staff staffMember in staffMembers)
        {
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

                data.Completions.Add(entry);
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

                data.Completions.Add(entry);
            }
        }

        return data;
    }
}
