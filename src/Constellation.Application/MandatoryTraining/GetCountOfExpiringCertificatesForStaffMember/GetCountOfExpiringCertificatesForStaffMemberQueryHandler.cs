namespace Constellation.Application.MandatoryTraining.GetCountOfExpiringCertificatesForStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCountOfExpiringCertificatesForStaffMemberQueryHandler 
    : IQueryHandler<GetCountOfExpiringCertificatesForStaffMemberQuery, int>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCountOfExpiringCertificatesForStaffMemberQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        ITrainingCompletionRepository trainingCompletionRepository,
        IStaffRepository staffRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _trainingCompletionRepository = trainingCompletionRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<int>> Handle(GetCountOfExpiringCertificatesForStaffMemberQuery request, CancellationToken cancellationToken)
    {
        var data = new StaffCompletionListDto();

        // - Get all modules
        var modules = await _trainingModuleRepository.GetCurrentModules(cancellationToken);

        // - Get staff member
        var staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        // - Get all completions for staff member
        var records = await _trainingCompletionRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);
            
        // - Build a collection of completions by each staff member for each module
        foreach (var record in records)
        {
            var entry = new CompletionRecordExtendedDetailsDto();
            entry.AddRecordDetails(record);

            entry.AddModuleDetails(record.Module);

            entry.AddStaffDetails(staff);

            data.Modules.Add(entry);
        }

        // Create blank entry for any missing records
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
            }
        }

        // - Remove all entries that are not due for expiry within 30 days, or have not already expired
        var latestRecords = data.Modules.Where(record => record.IsLatest && record.TimeToExpiry <= 30).ToList();

        return latestRecords.Count;
    }
}
