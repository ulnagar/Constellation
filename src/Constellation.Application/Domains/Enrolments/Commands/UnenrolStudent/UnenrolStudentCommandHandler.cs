﻿namespace Constellation.Application.Domains.Enrolments.Commands.UnenrolStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UnenrolStudentCommandHandler
    : ICommandHandler<UnenrolStudentCommand>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public UnenrolStudentCommandHandler(
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<UnenrolStudentCommand>();
    }

    public async Task<Result> Handle(UnenrolStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find Student with Id {id}", request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            if (enrolment.OfferingId == request.OfferingId)
            {
                Result actionRequest = enrolment.Cancel();

                if (actionRequest.IsFailure)
                {
                    return actionRequest;
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
