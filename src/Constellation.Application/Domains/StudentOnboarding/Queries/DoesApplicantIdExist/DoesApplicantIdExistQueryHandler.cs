namespace Constellation.Application.Domains.StudentOnboarding.Queries.DoesApplicantIdExist;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding.Repositories;
using Core.Shared;
using Serilog;

internal sealed class DoesApplicantIdExistQueryHandler
: IQueryHandler<DoesApplicantIdExistQuery, bool>
{
    private readonly IApplicantRepository _applicantRepository;
    private readonly ILogger _logger;

    public DoesApplicantIdExistQueryHandler(
        IApplicantRepository applicantRepository,
        ILogger logger)
    {
        _applicantRepository = applicantRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DoesApplicantIdExistQuery request, CancellationToken cancellationToken) 
        => await _applicantRepository.DoesApplicantIdExist(request.ApplicantId, cancellationToken);
}
