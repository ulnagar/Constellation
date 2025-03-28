namespace Constellation.Application.OfferingEnrolments.EnrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EnrolStudentCommandHandler
    : ICommandHandler<EnrolStudentCommand>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EnrolStudentCommandHandler(
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(EnrolStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Could not enrol student");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(EnrolStudentCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Could not enrol student");

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments.Any(enrolment => enrolment.OfferingId == request.OfferingId))
        {
            _logger
                .ForContext(nameof(EnrolStudentCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Enrolments.Enrolment.AlreadyExists(request.StudentId, request.OfferingId), true)
                .Warning("Could not enrol student");

            return Result.Failure(DomainErrors.Enrolments.Enrolment.AlreadyExists(request.StudentId, request.OfferingId));
        }

        OfferingEnrolment offeringEnrolment = OfferingEnrolment.Create(request.StudentId, request.OfferingId);

        _offeringEnrolmentRepository.Insert(offeringEnrolment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
