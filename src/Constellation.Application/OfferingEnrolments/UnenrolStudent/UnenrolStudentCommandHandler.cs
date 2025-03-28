namespace Constellation.Application.OfferingEnrolments.UnenrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UnenrolStudentCommandHandler
    : ICommandHandler<UnenrolStudentCommand>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public UnenrolStudentCommandHandler(
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
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

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        foreach (OfferingEnrolment enrolment in enrolments)
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
