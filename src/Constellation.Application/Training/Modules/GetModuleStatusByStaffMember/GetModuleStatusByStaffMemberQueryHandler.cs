namespace Constellation.Application.Training.Modules.GetModuleStatusByStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Training.Repositories;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Identifiers;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetModuleStatusByStaffMemberQueryHandler
    : IQueryHandler<GetModuleStatusByStaffMemberQuery, List<ModuleStatusResponse>>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetModuleStatusByStaffMemberQueryHandler(
        ITrainingRoleRepository roleRepository,
        ITrainingModuleRepository moduleRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _moduleRepository = moduleRepository;
        _dateTime = dateTime;
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

            TrainingCompletion completedRecord = module
                .Completions
                .Where(completion => completion.StaffId == request.StaffId)
                .MaxBy(completion => completion.CompletedDate);

            DateOnly? dueDate = null;

            if (required && completedRecord is null)
                dueDate = _dateTime.Today;

            if (required && completedRecord is not null && module.Expiry != TrainingModuleExpiryFrequency.OnceOff)
                dueDate = completedRecord.CompletedDate.AddYears((int)module.Expiry);

            response.Add(new(
                module.Id,
                module.Name,
                module.Expiry,
                required,
                roleList,
                completedRecord is not null,
                completedRecord?.Id,
                completedRecord?.CompletedDate,
                dueDate));
        }

        response = response.OrderBy(entry => entry.DueDate ?? DateOnly.MaxValue).ToList();

        return response;
    }
}
