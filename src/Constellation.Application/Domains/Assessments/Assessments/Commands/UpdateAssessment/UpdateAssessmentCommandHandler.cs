namespace Constellation.Application.Domains.Assessments.Assessments.Commands.UpdateAssessment;

using Abstractions.Messaging;
using Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessments;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using Serilog;

internal sealed class UpdateAssessmentCommandHandler
: ICommandHandler<UpdateAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateAssessmentCommand>();
    }

    public async Task<Result> Handle(UpdateAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.Id, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(UpdateAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.Id), true)
                .Warning("Failed to update Assessment");

            return Result.Failure<AssessmentResponse>(AssessmentErrors.NotFound(request.Id));
        }

        Course? course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(UpdateAssessmentCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Failed to update Assessment");

            return Result.Failure<AssessmentResponse>(CourseErrors.NotFound(request.CourseId));
        }

        assessment.Update(
            request.Name,
            course,
            request.DueDate,
            request.AvailableFrom,
            request.AvailableTo);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
