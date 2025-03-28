namespace Constellation.Application.OfferingEnrolments.UnenrolStudentFromAllOfferings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UnenrolStudentFromAllOfferingsCommandHandler
: ICommandHandler<UnenrolStudentFromAllOfferingsCommand>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UnenrolStudentFromAllOfferingsCommandHandler(
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UnenrolStudentFromAllOfferingsCommand request, CancellationToken cancellationToken)
    {
        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        foreach (OfferingEnrolment enrolment in enrolments)
        {
            enrolment.Cancel();
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
