namespace Constellation.Application.Domains.StudentOnboarding.Queries.DoesApplicationIdExist;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding.Repositories;
using Core.Shared;
using Serilog;

internal sealed class DoesApplicationIdExistQueryHandler
: IQueryHandler<DoesApplicationIdExistQuery, bool>
{
    private readonly IApplicantRepository _applicantRepository;
    private readonly ILogger _logger;

    public DoesApplicationIdExistQueryHandler(
        IApplicantRepository applicantRepository,
        ILogger logger)
    {
        _applicantRepository = applicantRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DoesApplicationIdExistQuery request, CancellationToken cancellationToken) 
        => await _applicantRepository.DoesApplicationIdExist(request.ApplicationId, cancellationToken);
}
