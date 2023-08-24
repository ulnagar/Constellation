namespace Constellation.Application.MandatoryTraining.GetListOfCompletionRecords;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCompletionRecordsQueryHandler 
    : IQueryHandler<GetListOfCompletionRecordsQuery, List<CompletionRecordDto>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;

    public GetListOfCompletionRecordsQueryHandler(
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ITrainingModuleRepository trainingModuleRepository)
    {
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _trainingModuleRepository = trainingModuleRepository;
    }

    public async Task<Result<List<CompletionRecordDto>>> Handle(GetListOfCompletionRecordsQuery request, CancellationToken cancellationToken)
    {
        List<CompletionRecordDto> data = new();
        List<Staff> staffMembers = new();

        if (string.IsNullOrWhiteSpace(request.StaffId))
            staffMembers = await _staffRepository.GetAllActive(cancellationToken);
        else
            staffMembers.Add(await _staffRepository.GetById(request.StaffId));

        List<TrainingModule> modules = await _trainingModuleRepository.GetAllCurrent(cancellationToken);

        foreach (Staff staffMember in staffMembers)
        {
            List<Guid> facultyIds = staffMember
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            List<Faculty> faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            foreach (TrainingModule module in modules)
            {
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
                    continue;

                CompletionRecordDto entry = new()
                {
                    Id = record.Id,
                    ModuleId = module.Id,
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

                data.Add(entry);
            }
        }

        return data;
    }
}
