﻿namespace Constellation.Application.Domains.Training.Queries.GetCountOfExpiringCertificatesForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Models;
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

        // Get Modules that include staff member
        List<TrainingModule> modules = await _trainingModuleRepository.GetModulesByAssignee(request.StaffId, cancellationToken);

        // - Get staff member
        StaffMember staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        foreach (TrainingModule module in modules)
        {
            if (module.IsDeleted) continue;

            TrainingCompletion record = module.Completions
                .Where(record =>
                    record.StaffId == staff.Id &&
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
