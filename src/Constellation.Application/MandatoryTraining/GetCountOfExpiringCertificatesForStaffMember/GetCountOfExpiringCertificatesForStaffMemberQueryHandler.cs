namespace Constellation.Application.MandatoryTraining.GetCountOfExpiringCertificatesForStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCountOfExpiringCertificatesForStaffMemberQueryHandler 
    : IQueryHandler<GetCountOfExpiringCertificatesForStaffMemberQuery, int>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCountOfExpiringCertificatesForStaffMemberQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<int>> Handle(GetCountOfExpiringCertificatesForStaffMemberQuery request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        // - Get all modules
        List<TrainingModule> modules = await _trainingModuleRepository.GetAllCurrent(cancellationToken);

        // - Get staff member
        Staff staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        foreach (TrainingModule module in modules)
        {
            List<TrainingCompletion> records = module.Completions
                .Where(record =>
                    record.StaffId == staff.StaffId &&
                    !record.IsDeleted)
                .ToList();

            TrainingCompletion record = records
                .OrderByDescending(record =>
                    (record.CompletedDate.HasValue) ? record.CompletedDate.Value : record.CreatedAt)
                .FirstOrDefault();

            CompletionRecordExtendedDetailsDto entry = new();
            entry.AddModuleDetails(module);
            entry.AddStaffDetails(staff);

            if (record is not null)
                entry.AddRecordDetails(record);

            entry.CalculateExpiry();

            data.Modules.Add(entry);
        }

        // - Count entried due for expiry within 30 days
        return data.Modules.Count(record => record.TimeToExpiry <= 30);
    }
}
