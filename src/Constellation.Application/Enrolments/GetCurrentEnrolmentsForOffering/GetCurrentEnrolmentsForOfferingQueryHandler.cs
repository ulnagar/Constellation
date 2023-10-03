namespace Constellation.Application.Enrolments.GetCurrentEnrolmentsForOffering;

using Abstractions.Messaging;
using Core.Models.Enrolments;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentEnrolmentsForOfferingQueryHandler
    : IQueryHandler<GetCurrentEnrolmentsForOfferingQuery, List<EnrolmentResponse>>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ILogger _logger;

    public GetCurrentEnrolmentsForOfferingQueryHandler(
        IEnrolmentRepository enrolmentRepository,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _logger = logger.ForContext<GetCurrentEnrolmentsForOfferingQuery>();
    }

    public async Task<Result<List<EnrolmentResponse>>> Handle(GetCurrentEnrolmentsForOfferingQuery request,
        CancellationToken cancellationToken)
    {
        List<EnrolmentResponse> response = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(request.OfferingId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            response.Add(new(
                enrolment.Id,
                enrolment.StudentId));
        }

        return response;
    }
}
