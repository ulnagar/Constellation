namespace Constellation.Application.OfferingEnrolments.EnrolMultipleStudentsInOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EnrolMultipleStudentsInOfferingCommandHandler
    : ICommandHandler<EnrolMultipleStudentsInOfferingCommand>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EnrolMultipleStudentsInOfferingCommandHandler(
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
        _logger = logger.ForContext<EnrolMultipleStudentsInOfferingCommand>();
    }

    public async Task<Result> Handle(EnrolMultipleStudentsInOfferingCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(EnrolMultipleStudentsInOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Could not create Enrolments for multiple Students");

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        foreach (StudentId studentId in request.StudentIds)
        {
            Student student = await _studentRepository.GetById(studentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(EnrolMultipleStudentsInOfferingCommand), request, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(studentId), true)
                    .Warning("Could not create Enrolments for multiple Students");

                continue;
            }

            OfferingEnrolment offeringEnrolment = OfferingEnrolment.Create(studentId, offering.Id);

            _offeringEnrolmentRepository.Insert(offeringEnrolment);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
