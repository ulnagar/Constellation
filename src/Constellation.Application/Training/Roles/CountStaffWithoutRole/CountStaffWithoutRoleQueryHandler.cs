namespace Constellation.Application.Training.Roles.CountStaffWithoutRole;

using Abstractions.Messaging;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStaffWithoutRoleQueryHandler
: IQueryHandler<CountStaffWithoutRoleQuery, int>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingRoleRepository _trainingRoleRepository;
    private readonly ILogger _logger;

    public CountStaffWithoutRoleQueryHandler(
        IStaffRepository staffRepository,
        ITrainingRoleRepository trainingRoleRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _trainingRoleRepository = trainingRoleRepository;
        _logger = logger.ForContext<CountStaffWithoutRoleQuery>();
    }

    public async Task<Result<int>> Handle(CountStaffWithoutRoleQuery request, CancellationToken cancellationToken)
    {
        int count = 0;

        List<string> staffIds = await _staffRepository.GetAllActiveStaffIds(cancellationToken);

        foreach (string staffId in staffIds)
        {
            List<TrainingRole> roles = await _trainingRoleRepository.GetRolesForStaffMember(staffId, cancellationToken);

            if (roles.Count == 0)
                count++;
        }

        return count;
    }
}
