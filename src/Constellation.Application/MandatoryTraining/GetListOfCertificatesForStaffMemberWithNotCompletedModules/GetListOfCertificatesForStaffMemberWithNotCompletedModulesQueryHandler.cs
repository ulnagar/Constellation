namespace Constellation.Application.MandatoryTraining.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler 
    : IQueryHandler<GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery, StaffCompletionListDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IStaffRepository _staffRepository;

    public GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        ITrainingCompletionRepository trainingCompletionRepository,
        IStaffRepository staffRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _trainingCompletionRepository = trainingCompletionRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<StaffCompletionListDto>> Handle(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery request, CancellationToken cancellationToken)
    {
        var data = new StaffCompletionListDto();

        // - Get all modules
        var modules = await _trainingModuleRepository.GetCurrentModules(cancellationToken);

        // - Get staff member
        var staff = await _staffRepository.GetById(request.StaffId, cancellationToken);
            
        // - Get completions for staff member
        var records = await _trainingCompletionRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);
            
        data.StaffId = request.StaffId;
        data.Name = staff.DisplayName;
        data.SchoolName = staff.School.Name;
        data.EmailAddress = staff.EmailAddress;
        data.Faculties = staff.Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty.Name).ToList();

        // - Build a collection of completions by each staff member for each module
        foreach (var record in records)
        {
            var entry = new CompletionRecordExtendedDetailsDto();
            entry.AddRecordDetails(record);

            entry.AddModuleDetails(record.Module);

            entry.AddStaffDetails(staff);

            data.Modules.Add(entry);
        }

        // Create blank entries for any missing modules
        foreach (var module in modules)
        {
            if (data.Modules.Any(record => record.ModuleId == module.Id))
                continue;

            var entry = new CompletionRecordExtendedDetailsDto();
            entry.AddModuleDetails(module);

            entry.AddStaffDetails(staff);

            entry.RecordEffectiveDate = null;
            entry.RecordNotRequired = false;

            data.Modules.Add(entry);
        }

        var duplicateEntries = new List<CompletionRecordExtendedDetailsDto>();

        // - Remove all superceded entries
        foreach (var record in data.Modules)
        {
            var duplicates = data.Modules.Where(entry =>
                    entry.ModuleId == record.ModuleId &&
                    entry.StaffId == record.StaffId &&
                    entry.RecordId != record.RecordId)
                .ToList();

            if (duplicates.All(entry => entry.RecordEffectiveDate < record.RecordEffectiveDate))
            {
                record.IsLatest = true;
                record.CalculateExpiry();

                foreach (var duplicate in duplicates)
                {
                    duplicateEntries.Add(duplicate);
                }
            }
        }

        foreach (var entry in duplicateEntries)
        {
            data.Modules.Remove(entry);
        }

        return data;
    }
}
