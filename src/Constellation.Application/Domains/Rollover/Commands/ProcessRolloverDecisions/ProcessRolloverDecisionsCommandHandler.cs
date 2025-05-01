namespace Constellation.Application.Domains.Rollover.Commands.ProcessRolloverDecisions;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.Rollover;
using Core.Models.Rollover.Enums;
using Core.Models.Rollover.Errors;
using Core.Models.Rollover.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessRolloverDecisionsCommandHandler
    : ICommandHandler<ProcessRolloverDecisionsCommand, List<RolloverResult>>
{
    private readonly IRolloverRepository _rolloverRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ProcessRolloverDecisionsCommandHandler(
        IRolloverRepository rolloverRepository, 
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _rolloverRepository = rolloverRepository;
        _studentRepository = studentRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ProcessRolloverDecisionsCommand>();
    }

    public async Task<Result<List<RolloverResult>>> Handle(ProcessRolloverDecisionsCommand request, CancellationToken cancellationToken)
    {
        List<RolloverResult> results = new();

        List<RolloverDecision> decisions = _rolloverRepository.GetRegisteredDecisions();

        foreach (RolloverDecision decision in decisions)
        {
            if (decision.Decision == RolloverStatus.Unknown)
            {
                results.Add(new(decision, Result.Failure(RolloverErrors.InvalidDecision)));
            }

            Student student = await _studentRepository.GetById(decision.StudentId, cancellationToken);
            
            if (student is null)
            {
                _logger
                    .ForContext(nameof(RolloverDecision), decision, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(decision.StudentId), true)
                    .Warning("Could not process Rollover Decision for student");

                results.Add(new(decision, Result.Failure(StudentErrors.NotFound(decision.StudentId))));

                continue;
            }

            if (decision.Decision == RolloverStatus.Withdraw)
            {
                student.Withdraw(_dateTime);

                results.Add(new(decision, Result.Success()));
            }

            if (decision.Decision == RolloverStatus.Rollover)
            {
                SchoolEnrolment? enrolment = student.CurrentEnrolment;

                if (enrolment is null)
                {
                    _logger
                        .ForContext(nameof(RolloverDecision), decision, true)
                        .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                        .Warning("Could not process Rollover Decision for student");

                    results.Add(new(decision, Result.Failure(SchoolEnrolmentErrors.NotFound)));

                    continue;
                }

                if (enrolment.Grade == Grade.Y12)
                {
                    _logger
                        .ForContext(nameof(RolloverDecision), decision, true)
                        .ForContext(nameof(Error), RolloverErrors.RolloverImpossible, true)
                        .Warning("Could not process Rollover Decision for student");

                    results.Add(new(decision, Result.Failure(RolloverErrors.RolloverImpossible)));
                    
                    continue;
                }

                if (enrolment.Grade == Grade.Y06)
                {
                    student.RemoveSchoolEnrolment(enrolment, _dateTime);
                }
                else
                {
                    Result newEnrolment = student.AddSchoolEnrolment(
                        enrolment.SchoolCode,
                        enrolment.SchoolName,
                        enrolment.Grade + 1,
                        _dateTime);

                    if (newEnrolment.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(RolloverDecision), decision, true)
                            .ForContext(nameof(Error), newEnrolment.Error, true)
                            .Warning("Could not process Rollover Decision for student");

                        results.Add(new(decision, Result.Failure(newEnrolment.Error)));

                        continue;
                    }
                }

                results.Add(new(decision, Result.Success()));
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _rolloverRepository.Reset();

        return results;
    }
}
