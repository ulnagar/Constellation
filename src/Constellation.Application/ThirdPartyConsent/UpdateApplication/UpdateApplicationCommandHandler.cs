namespace Constellation.Application.ThirdPartyConsent.UpdateApplication;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateApplicationCommandHandler
    : ICommandHandler<UpdateApplicationCommand>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateApplicationCommandHandler(
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateApplicationCommand>();
    }

    public async Task<Result> Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.Id, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(UpdateApplicationCommand), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.Id), true)
                .Warning("Failed to update Consent Application");

            return Result.Failure(ConsentApplicationErrors.NotFound(request.Id));
        }

        application.Update(
            request.Name,
            request.Purpose,
            request.InformationCollected,
            request.StoredCountry,
            request.SharedWith,
            request.ConsentRequired);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
