namespace Constellation.Application.Domains.ThirdPartyConsent.Commands.ReenableApplication;

using Abstractions.Messaging;
using Constellation.Application.Domains.ThirdPartyConsent.Commands.DisableApplication;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReenableApplicationCommandHandler
    : ICommandHandler<ReenableApplicationCommand>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReenableApplicationCommandHandler(
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ReenableApplicationCommand request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(DisableApplicationCommand), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to reenable Consent Application");

            return Result.Failure(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        application.Reenable();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
