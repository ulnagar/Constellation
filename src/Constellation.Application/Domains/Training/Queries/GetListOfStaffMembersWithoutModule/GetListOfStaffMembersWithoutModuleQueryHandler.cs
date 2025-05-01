namespace Constellation.Application.Domains.Training.Queries.GetListOfStaffMembersWithoutModule;

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

internal sealed class GetListOfStaffMembersWithoutModuleQueryHandler
: IQueryHandler<GetListOfStaffMembersWithoutModuleQuery, List<StaffResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ILogger _logger;

    public GetListOfStaffMembersWithoutModuleQueryHandler(
        IStaffRepository staffRepository,
        ITrainingModuleRepository moduleRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _moduleRepository = moduleRepository;
        _logger = logger.ForContext<GetListOfStaffMembersWithoutModuleQuery>();
    }

    public async Task<Result<List<StaffResponse>>> Handle(GetListOfStaffMembersWithoutModuleQuery request, CancellationToken cancellationToken)
    {
        List<StaffResponse> response = new();

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

        modules = modules.Where(entry => !entry.IsDeleted).ToList();

        foreach (Staff staffMember in staffMembers)
        {
            int staffModules = modules.Count(module => module.Assignees.Any(entry => entry.StaffId == staffMember.StaffId));

            if (staffModules == 0)
            {
                response.Add(new(
                    staffMember.StaffId,
                    staffMember.GetName()));
            }
        }

        return response;
    }
}
