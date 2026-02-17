namespace Constellation.Infrastructure.Jobs;

using Application.Domains.AppSettings.Models;
using Application.Domains.WorkFlows.Commands.AddCaseDetailUpdateAction;
using Application.Domains.WorkFlows.Commands.CreateTrainingCase;
using Application.Interfaces.Services;
using Constellation.Application.Interfaces.Jobs;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.AppSettings.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MandatoryTrainingScanJob : IMandatoryTrainingScanJob
{
    private readonly IAppSettingsService _appSettings;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICaseRepository _caseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public MandatoryTrainingScanJob(
        IAppSettingsService appSettings,
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        ICaseRepository caseRepository,
        IDateTimeProvider dateTime,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _caseRepository = caseRepository;
        _dateTime = dateTime;
        _mediator = mediator;
        _logger = logger.ForContext<IMandatoryTrainingScanJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        // Find all staff with required modules that are due in 30 days, 14 days, 1 day, and overdue.
        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        List<TrainingModule> modules = await _trainingModuleRepository.GetAllModules(cancellationToken);

        foreach (StaffMember staffMember in staff)
        {
            _logger.Information("Checking {user}",staffMember.Name.DisplayName);

            // Get all roles for the staff member
            List<TrainingModule> staffModules = modules
                .Where(module =>
                    module.Assignees.Any(entry =>
                        entry.StaffId == staffMember.Id))
                .ToList();

            // Get all completions for staff member
            foreach (TrainingModule module in staffModules)
            {
                if (module.IsDeleted) continue;

                TrainingCompletion? completion = module
                    .Completions
                    .Where(record =>
                        record.StaffId == staffMember.Id &&
                        !record.IsDeleted)
                    .MaxBy(record => record.CompletedDate);

                if (module.Expiry == TrainingModuleExpiryFrequency.OnceOff && completion is not null)
                    continue;

                DateOnly dueDate = (completion is null)
                    ? _dateTime.Today
                    : completion.CompletedDate.AddYears((int)module.Expiry);

                int countdown = (int)dueDate.ToDateTime(TimeOnly.MinValue)
                    .Subtract(_dateTime.Today.ToDateTime(TimeOnly.MinValue)).TotalDays;

                if (countdown > 30)
                    continue;

                _logger
                    .ForContext(nameof(TrainingModule), module.Name)
                    .ForContext(nameof(TrainingCaseDetail.DueDate), dueDate)
                    .Information($" Found Module nearing due date");
                    
                // Check for an existing WorkFlow for these items, and create one if not existing
                Case? existingCase = await _caseRepository.GetTrainingCaseForStaffAndModule(staffMember.Id, module.Id, cancellationToken);

                if (existingCase is null)
                {
                    await _mediator.Send(new CreateTrainingCaseCommand(staffMember.Id, module.Id, completion?.Id), cancellationToken);
                        
                    continue;
                }

                if (countdown is not (30 or 14 or 0 or -7 or -30)) 
                    continue;

                WorkflowConfiguration? configuration = await _appSettings.Workflow(WorkflowArea.Training, cancellationToken);

                if (configuration is null)
                {
                    _logger
                        .Warning("Failed to send lesson notification");

                    return;
                }

                StaffMember? reviewer = null;

                foreach (var entry in configuration.Contacts)
                    reviewer = entry.Key;

                if (reviewer is null)
                {
                    _logger
                        .Warning("Failed to send lesson notification");

                    return;
                }

                // force a reminder email update
                AddCaseDetailUpdateActionCommand command = new(
                    existingCase.Id,
                    reviewer.EmployeeId,
                    $"Module identified during scan on {_dateTime.Today.ToShortDateString()} as being due for completion in {countdown} days.");

                await _mediator.Send(command, cancellationToken);
            }
        }
    }
}