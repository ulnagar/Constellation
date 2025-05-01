namespace Constellation.Application.Domains.Training.Queries.CountStaffWithoutModule;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStaffWithoutModuleQueryHandler
: IQueryHandler<CountStaffWithoutModuleQuery, int>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ILogger _logger;

    public CountStaffWithoutModuleQueryHandler(
        IStaffRepository staffRepository,
        ITrainingModuleRepository moduleRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _moduleRepository = moduleRepository;
        _logger = logger.ForContext<CountStaffWithoutModuleQuery>();
    }

    public async Task<Result<int>> Handle(CountStaffWithoutModuleQuery request, CancellationToken cancellationToken)
    {
        int staffWithoutModule = 0;

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

        modules = modules.Where(entry => !entry.IsDeleted).ToList();

        foreach (Staff staffMember in staffMembers)
        {
            int staffModules = modules.Count(module => module.Assignees.Any(entry => entry.StaffId == staffMember.StaffId));

            if (staffModules == 0)
                staffWithoutModule++;
        }

        return staffWithoutModule;
    }
}
