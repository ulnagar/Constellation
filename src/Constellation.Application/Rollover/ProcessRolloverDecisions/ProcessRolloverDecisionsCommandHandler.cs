namespace Constellation.Application.Rollover.ProcessRolloverDecisions;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Core.Enums;
using Core.Models.Rollover;
using Core.Models.Rollover.Enums;
using Core.Models.Rollover.Errors;
using Core.Models.Rollover.Repositories;
using Core.Models.Students.Errors;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ProcessRolloverDecisionsCommandHandler(
        IRolloverRepository rolloverRepository, 
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _rolloverRepository = rolloverRepository;
        _studentRepository = studentRepository;
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
                student.Withdraw();

                results.Add(new(decision, Result.Success()));
            }

            if (decision.Decision == RolloverStatus.Rollover)
            {
                if (student.CurrentGrade == Grade.Y12)
                {
                    _logger
                        .ForContext(nameof(RolloverDecision), decision, true)
                        .ForContext(nameof(Error), RolloverErrors.RolloverImpossible, true)
                        .Warning("Could not process Rollover Decision for student");

                    results.Add(new(decision, Result.Failure(RolloverErrors.RolloverImpossible)));
                    
                    continue;
                }

                switch (student.CurrentGrade)
                {
                    case Grade.Y11:
                        student.CurrentGrade = Grade.Y12;
                        break;
                    case Grade.Y10:
                        student.CurrentGrade = Grade.Y11;
                        break;
                    case Grade.Y09:
                        student.CurrentGrade = Grade.Y10;
                        break;
                    case Grade.Y08:
                        student.CurrentGrade = Grade.Y09;
                        break;
                    case Grade.Y07:
                        student.CurrentGrade = Grade.Y08;
                        break;
                    case Grade.Y06:
                        student.CurrentGrade = Grade.Y07;
                        student.SchoolCode = "8912";
                        break;
                    case Grade.Y05:
                        student.CurrentGrade = Grade.Y06;
                        break;
                }

                results.Add(new(decision, Result.Success()));
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _rolloverRepository.Reset();

        return results;
    }
}
