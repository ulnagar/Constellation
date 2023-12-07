namespace Constellation.Application.Training.Roles.GetTrainingRoleDetails;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTrainingRoleDetailsQueryHandler
: IQueryHandler<GetTrainingRoleDetailsQuery, TrainingRoleDetailResponse>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetTrainingRoleDetailsQueryHandler(
        ITrainingRoleRepository roleRepository,
        ITrainingModuleRepository moduleRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _moduleRepository = moduleRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    public async Task<Result<TrainingRoleDetailResponse>> Handle(GetTrainingRoleDetailsQuery request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(GetTrainingRoleDetailsQuery), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to retrieve Role details");

            return Result.Failure<TrainingRoleDetailResponse>(TrainingErrors.Role.NotFound(request.RoleId));
        }

        List<TrainingRoleDetailResponse.RoleModule> modules = new();

        foreach (TrainingRoleModule moduleEntry in role.Modules)
        {
            TrainingModule module = await _moduleRepository.GetModuleById(moduleEntry.ModuleId, cancellationToken);

            if (module is null)
            {
                _logger
                    .ForContext(nameof(GetTrainingRoleDetailsQuery), request, true)
                    .ForContext(nameof(Error), TrainingErrors.Module.NotFound(moduleEntry.ModuleId), true)
                    .Warning("Failed to retrieve Role details");

                continue;
            }

            modules.Add(new(
                module.Id,
                module.Name,
                module.Expiry));
        }

        List<TrainingRoleDetailResponse.RoleMember> members = new();

        foreach (TrainingRoleMember memberEntry in role.Members)
        {
            Staff staffMember = await _staffRepository.GetById(memberEntry.StaffId, cancellationToken);

            if (staffMember is null)
            {
                _logger
                    .ForContext(nameof(GetTrainingRoleDetailsQuery), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(memberEntry.StaffId), true)
                    .Warning("Failed to retrieve Role details");

                continue;
            }

            School school = await _schoolRepository.GetById(staffMember.SchoolCode, cancellationToken);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(GetTrainingRoleDetailsQuery), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(memberEntry.StaffId), true)
                    .Warning("Failed to retrieve Role details");

                continue;
            }

            members.Add(new(
                staffMember.StaffId,
                staffMember.GetName(),
                school.Name));
        }

        return new TrainingRoleDetailResponse(
            role.Id,
            role.Name,
            members,
            modules);
    }
}
