namespace Constellation.Application.Domains.Training.Queries.GetModuleStatusByStaffMember;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.Training;
using Core.Models.Training.Repositories;
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
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetModuleStatusByStaffMemberQueryHandler(
        ITrainingModuleRepository moduleRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<GetModuleStatusByStaffMemberQuery>();
    }

    public async Task<Result<List<ModuleStatusResponse>>> Handle(GetModuleStatusByStaffMemberQuery request, CancellationToken cancellationToken)
    {
        List<ModuleStatusResponse> response = new();

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

        foreach (TrainingModule module in modules.OrderBy(module => module.Name))
        {
            if (module.IsDeleted) continue;

            bool required = module.Assignees.Any(entry => entry.StaffId == request.StaffId);

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
                completedRecord is not null,
                completedRecord?.Id,
                completedRecord?.CompletedDate,
                dueDate));
        }

        response = response.OrderBy(entry => entry.DueDate ?? DateOnly.MaxValue).ToList();

        return response;
    }
}
