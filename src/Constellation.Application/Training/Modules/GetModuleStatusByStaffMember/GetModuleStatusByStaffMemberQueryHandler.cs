namespace Constellation.Application.Training.Modules.GetModuleStatusByStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Training.Repositories;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Identifiers;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetModuleStatusByStaffMemberQueryHandler
    : IQueryHandler<GetModuleStatusByStaffMemberQuery, List<ModuleStatusResponse>>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ILogger _logger;

    public GetModuleStatusByStaffMemberQueryHandler(
        ITrainingRoleRepository roleRepository,
        ITrainingModuleRepository moduleRepository,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _moduleRepository = moduleRepository;
        _logger = logger.ForContext<GetModuleStatusByStaffMemberQuery>();
    }

    public async Task<Result<List<ModuleStatusResponse>>> Handle(GetModuleStatusByStaffMemberQuery request, CancellationToken cancellationToken)
    {
        List<ModuleStatusResponse> response = new();

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

        List<TrainingRole> roles = await _roleRepository.GetRolesForStaffMember(request.StaffId, cancellationToken);

        foreach (TrainingModule module in modules.OrderBy(module => module.Name))
        {
            bool required = roles.SelectMany(role => role.Modules).Any(entry => entry.ModuleId == module.Id);

            Dictionary<TrainingRoleId, string> roleList = required
                ? roles
                    .Where(role => 
                        role.Modules.Any(entry => 
                            entry.ModuleId == module.Id))
                    .ToDictionary(k => k.Id, k => k.Name)
                : new();

            response.Add(new(
                module.Id,
                module.Name,
                module.Expiry,
                required,
                roleList));
        }

        return response;
    }
}
