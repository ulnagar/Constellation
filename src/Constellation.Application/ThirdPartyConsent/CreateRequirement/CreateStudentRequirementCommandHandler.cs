namespace Constellation.Application.ThirdPartyConsent.CreateRequirement;

using Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateStudentRequirementCommandHandler
: ICommandHandler<CreateStudentRequirementCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateStudentRequirementCommandHandler(
        IStudentRepository studentRepository,
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateStudentRequirementCommand>();
    }

    public async Task<Result> Handle(CreateStudentRequirementCommand request, CancellationToken cancellationToken)
    {
        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(CreateStudentRequirementCommand), request, true)
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to create Student Requirement for Application");

            return Result.Failure(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateStudentRequirementCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to create Student Requirement for Application");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        ConsentRequirement existingRequirement = application.Requirements
            .OfType<StudentConsentRequirement>()
            .FirstOrDefault(entry =>
                entry.StudentId == request.StudentId &&
                !entry.IsDeleted);

        if (existingRequirement is not null)
        {
            _logger
                .ForContext(nameof(CreateStudentRequirementCommand), request, true)
                .ForContext(nameof(CourseConsentRequirement), existingRequirement, true)
                .ForContext(nameof(Error), ConsentRequirementErrors.AlreadyExists(typeof(StudentConsentRequirement), request.StudentId.ToString()), true)
                .Warning("Failed to create Student Requirement for Application");

            return Result.Failure(ConsentRequirementErrors.AlreadyExists(typeof(StudentConsentRequirement), request.StudentId.ToString()));
        }

        Result<StudentConsentRequirement> requirement = StudentConsentRequirement.Create(
            student,
            application);

        if (requirement.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateStudentRequirementCommand), request, true)
                .ForContext(nameof(Error), requirement.Error, true)
                .Warning("Failed to create Student Requirement for Application");

            return Result.Failure(requirement.Error);
        }

        application.AddConsentRequirement(requirement.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
