namespace Constellation.Application.ThirdPartyConsent.CreateApplication;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateApplicationCommandHandler
    : ICommandHandler<CreateApplicationCommand, ApplicationId>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateApplicationCommandHandler(
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ApplicationId>> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        Application application = Application.Create(
            request.Name,
            request.Purpose,
            request.InformationCollected,
            request.StoredCountry,
            request.SharedWith,
            request.ConsentRequired);

        _consentRepository.Insert(application);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return application.Id;
    }
}