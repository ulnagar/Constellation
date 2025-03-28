namespace Constellation.Application.OfferingEnrolments.GetCurrentEnrolmentsForOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentEnrolmentsForOfferingQueryHandler
    : IQueryHandler<GetCurrentEnrolmentsForOfferingQuery, List<EnrolmentResponse>>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly ILogger _logger;

    public GetCurrentEnrolmentsForOfferingQueryHandler(
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        ILogger logger)
    {
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _logger = logger.ForContext<GetCurrentEnrolmentsForOfferingQuery>();
    }

    public async Task<Result<List<EnrolmentResponse>>> Handle(GetCurrentEnrolmentsForOfferingQuery request,
        CancellationToken cancellationToken)
    {
        List<EnrolmentResponse> response = new();

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByOfferingId(request.OfferingId, cancellationToken);

        foreach (OfferingEnrolment enrolment in enrolments)
        {
            response.Add(new(
                enrolment.Id,
                enrolment.StudentId));
        }

        return response;
    }
}
