namespace Constellation.Application.MandatoryTraining.GetListOfCompletionRecords;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCompletionRecordsQueryHandler 
    : IQueryHandler<GetListOfCompletionRecordsQuery, List<CompletionRecordDto>>
{
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;

    public GetListOfCompletionRecordsQueryHandler(
        ITrainingCompletionRepository trainingCompletionRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ITrainingModuleRepository trainingModuleRepository)
    {
        _trainingCompletionRepository = trainingCompletionRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _trainingModuleRepository = trainingModuleRepository;
    }

    public async Task<Result<List<CompletionRecordDto>>> Handle(GetListOfCompletionRecordsQuery request, CancellationToken cancellationToken)
    {
        List<CompletionRecordDto> data = new();
        List<TrainingCompletion> records = new();

        if (string.IsNullOrWhiteSpace(request.StaffId))
            records = await _trainingCompletionRepository.GetAllCurrent(cancellationToken);
        else
            records = await _trainingCompletionRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        foreach (var staffMemberRecords in records.GroupBy(record => record.StaffId))
        {
            var staff = await _staffRepository.GetByIdWithFacultyMemberships(staffMemberRecords.Key, cancellationToken);

            var facultyIds = staff
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            var faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            foreach (var record in staffMemberRecords)
            {
                var module = await _trainingModuleRepository.GetById(record.TrainingModuleId, cancellationToken);

                var entry = new CompletionRecordDto
                {
                    Id = record.Id,
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    ModuleExpiry = module.Expiry,
                    StaffId = record.StaffId,
                    StaffFirstName = staff.FirstName,
                    StaffLastName = staff.LastName,
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

        foreach (var record in data)
        {
            if (data.Any(other =>
                other.Id != record.Id &&
                other.ModuleId == record.ModuleId && // true
                other.StaffId == record.StaffId && // true
                (other.NotRequired && other.CreatedAt > record.CompletedDate || // false
                !other.NotRequired && !record.NotRequired && other.CompletedDate > record.CompletedDate || // false
                record.NotRequired && record.CreatedAt < other.CompletedDate))) // false
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superceded;
            }
        }

        return data;
    }
}
