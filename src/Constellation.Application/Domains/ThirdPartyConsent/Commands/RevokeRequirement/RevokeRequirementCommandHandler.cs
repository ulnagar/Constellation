namespace Constellation.Application.Domains.ThirdPartyConsent.Commands.RevokeRequirement;

using Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RevokeRequirementCommandHandler
: ICommandHandler<RevokeRequirementCommand>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RevokeRequirementCommandHandler(
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RevokeRequirementCommand>();
    }

    public async Task<Result> Handle(RevokeRequirementCommand request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(RevokeRequirementCommand), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to remove Course Requirement for Application");

            return Result.Failure(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        ConsentRequirement existingRequirement = application.Requirements
            .FirstOrDefault(entry =>
                entry.Id == request.RequirementId);

        if (existingRequirement is null)
        {
            _logger
                .ForContext(nameof(RevokeRequirementCommand), request, true)
                .ForContext(nameof(Error), ConsentRequirementErrors.NotFound(request.ApplicationId, request.RequirementId), true)
                .Warning("Failed to remove Course Requirement for Application");

            return Result.Failure(ConsentRequirementErrors.NotFound(request.ApplicationId, request.RequirementId));
        }

        existingRequirement.Delete();
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
