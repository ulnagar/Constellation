namespace Constellation.Application.ThirdPartyConsent.CreateRequirement;

using Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Core.Extensions;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateGradeRequirementCommandHandler
    : ICommandHandler<CreateGradeRequirementCommand>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateGradeRequirementCommandHandler(
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateGradeRequirementCommand>();
    }

    public async Task<Result> Handle(CreateGradeRequirementCommand request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(CreateGradeRequirementCommand), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to create Grade Requirement for Application");

            return Result.Failure(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }
        
        ConsentRequirement existingRequirement = application.Requirements
            .OfType<GradeConsentRequirement>()
            .FirstOrDefault(entry =>
                entry.Grade == request.Grade &&
                !entry.IsDeleted);

        if (existingRequirement is not null)
        {
            _logger
                .ForContext(nameof(CreateGradeRequirementCommand), request, true)
                .ForContext(nameof(CourseConsentRequirement), existingRequirement, true)
                .ForContext(nameof(Error), ConsentRequirementErrors.AlreadyExists(typeof(GradeConsentRequirement), request.Grade.AsName()), true)
                .Warning("Failed to create Grade Requirement for Application");

            return Result.Failure(ConsentRequirementErrors.AlreadyExists(typeof(GradeConsentRequirement), request.Grade.AsName()));
        }

        Result<GradeConsentRequirement> requirement = GradeConsentRequirement.Create(
            request.Grade,
            application);

        if (requirement.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateGradeRequirementCommand), request, true)
                .ForContext(nameof(Error), requirement.Error, true)
                .Warning("Failed to create Grade Requirement for Application");

            return Result.Failure(requirement.Error);
        }

        application.AddConsentRequirement(requirement.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
