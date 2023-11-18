namespace Constellation.Application.MandatoryTraining.GetModuleDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Core.Models.Faculty.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetModuleDetailsQueryHandler 
    : IQueryHandler<GetModuleDetailsQuery, ModuleDetailsDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;

    public GetModuleDetailsQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<ModuleDetailsDto>> Handle(GetModuleDetailsQuery request, CancellationToken cancellationToken)
    {
        ModuleDetailsDto data = new();

        // Get info from database
        TrainingModule module = await _trainingModuleRepository.GetById(request.Id, cancellationToken);

        data.Id = module.Id;
        data.Name = module.Name;
        data.Expiry = module.Expiry.GetDisplayName();
        data.Url = (string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url);
        data.IsActive = !module.IsDeleted;

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

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

            TrainingCompletion record = records
                .OrderByDescending(record =>
                    (record.CompletedDate.HasValue) ? record.CompletedDate.Value : record.CreatedAt)
                .FirstOrDefault();

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
                    NotRequired = record.NotRequired,
                    CreatedAt = record.CreatedAt
                };

                entry.ExpiryCountdown = entry.CalculateExpiry();
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;

                data.Completions.Add(entry);
            }
        }

        return data;
    }
}
