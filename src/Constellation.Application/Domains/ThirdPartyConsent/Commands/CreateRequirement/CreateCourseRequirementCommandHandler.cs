namespace Constellation.Application.Domains.ThirdPartyConsent.Commands.CreateRequirement;

using Abstractions.Messaging;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Repositories;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateCourseRequirementCommandHandler
    : ICommandHandler<CreateCourseRequirementCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCourseRequirementCommandHandler(
        ICourseRepository courseRepository,
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateCourseRequirementCommand>();
    }

    public async Task<Result> Handle(CreateCourseRequirementCommand request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(CreateCourseRequirementCommand), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to create Course Requirement for Application");

            return Result.Failure(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(CreateCourseRequirementCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Failed to create Course Requirement for Application");

            return Result.Failure(CourseErrors.NotFound(request.CourseId));
        }

        ConsentRequirement existingRequirement = application.Requirements
            .OfType<CourseConsentRequirement>()
            .FirstOrDefault(entry =>
                entry.CourseId == request.CourseId &&
                !entry.IsDeleted);

        if (existingRequirement is not null)
        {
            _logger
                .ForContext(nameof(CreateCourseRequirementCommand), request, true)
                .ForContext(nameof(CourseConsentRequirement), existingRequirement, true)
                .ForContext(nameof(Error), ConsentRequirementErrors.AlreadyExists(typeof(CourseConsentRequirement), request.CourseId.ToString()), true)
                .Warning("Failed to create Course Requirement for Application");

            return Result.Failure(ConsentRequirementErrors.AlreadyExists(typeof(CourseConsentRequirement), request.CourseId.ToString()));
        }

        Result<CourseConsentRequirement> requirement = CourseConsentRequirement.Create(
            course,
            application);

        if (requirement.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateCourseRequirementCommand), request, true)
                .ForContext(nameof(Error), requirement.Error, true)
                .Warning("Failed to create Course Requirement for Application");

            return Result.Failure(requirement.Error);
        }

        application.AddConsentRequirement(requirement.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
