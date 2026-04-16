namespace Constellation.Application.Domains.Assessments.Assessments.Commands.CreateAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class CreateAssessmentCommandHandler
: ICommandHandler<CreateAssessmentCommand, AssessmentId>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateAssessmentCommand>();
    }

    public async Task<Result<AssessmentId>> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        Course? course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(CreateAssessmentCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Failed to create new Assessment");

            return Result.Failure<AssessmentId>(CourseErrors.NotFound(request.CourseId));
        }

        Assessment assessment = new(
            request.Name,
            course,
            request.DueDate,
            request.AvailableFrom,
            request.AvailableTo);

        _assessmentRepository.Insert(assessment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return assessment.Id;
    }
}
