namespace Constellation.Application.Domains.StudentOnboarding.Queries.DoesApplicationIdExist;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding.Repositories;
using Core.Shared;
using Serilog;

internal sealed class DoesApplicationIdExistQueryHandler
: IQueryHandler<DoesApplicationIdExistQuery, bool>
{
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly ILogger _logger;

    public DoesApplicationIdExistQueryHandler(
        IOnboardingRepository onboardingRepository,
        ILogger logger)
    {
        _onboardingRepository = onboardingRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DoesApplicationIdExistQuery request, CancellationToken cancellationToken) 
        => await _onboardingRepository.DoesApplicationIdExist(request.ApplicationId, cancellationToken);
}
