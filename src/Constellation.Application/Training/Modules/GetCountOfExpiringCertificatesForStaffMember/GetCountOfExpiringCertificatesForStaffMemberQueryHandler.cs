namespace Constellation.Application.Training.Modules.GetCountOfExpiringCertificatesForStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Shared;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCountOfExpiringCertificatesForStaffMemberQueryHandler
    : IQueryHandler<GetCountOfExpiringCertificatesForStaffMemberQuery, int>
{
    private readonly ITrainingRoleRepository _trainingRoleRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCountOfExpiringCertificatesForStaffMemberQueryHandler(
        ITrainingRoleRepository trainingRoleRepository,
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository)
    {
        _trainingRoleRepository = trainingRoleRepository;
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<int>> Handle(GetCountOfExpiringCertificatesForStaffMemberQuery request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        // Get Roles that include staff member
        List<TrainingRole> roles = await _trainingRoleRepository.GetRolesForStaffMember(request.StaffId, cancellationToken);

        // Get Modules attached to roles
        List<TrainingModuleId> moduleIds = roles
            .SelectMany(role => role.Modules)
            .Select(module => module.ModuleId)
            .ToList();

        // - Get staff member
        Staff staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        foreach (TrainingModuleId moduleId in moduleIds)
        {
            TrainingModule module = await _trainingModuleRepository.GetModuleById(moduleId, cancellationToken);

            TrainingCompletion record = module.Completions
                .Where(record =>
                    record.StaffId == staff.StaffId &&
                    !record.IsDeleted)
                .MaxBy(record => record.CompletedDate);

            CompletionRecordExtendedDetailsDto entry = new();
            entry.AddModuleDetails(module);
            entry.AddStaffDetails(staff);

            if (record is not null)
                entry.AddRecordDetails(record);

            entry.CalculateExpiry();

            data.Modules.Add(entry);
        }

        // - Count entries due for expiry within 30 days
        return data.Modules.Count(record => record.TimeToExpiry <= 30);
    }
}
