namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDetailsById;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetAssessmentDetailsByIdQueryHandler
: IQueryHandler<GetAssessmentDetailsByIdQuery, AssessmentDetailsResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetAssessmentDetailsByIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger;
    }

    public async Task<Result<AssessmentDetailsResponse>> Handle(GetAssessmentDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.Id, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentDetailsByIdQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Assessment");

            return Result.Failure<AssessmentDetailsResponse>(AssessmentErrors.NotFound(request.Id));
        }

        List<AssessmentDetailsResponse.Student> students = [];
        List<AssessmentDetailsResponse.Download> downloads = [];
        List<AssessmentDetailsResponse.Submission> submissions = [];
        List<AssessmentDetailsResponse.Instruction> instructions = [];

        foreach (AssessmentStudent student in assessment.Students)
        {
            students.Add(new(
                student.StudentId,
                student.Student,
                student.StudentGrade,
                student.SchoolCode,
                student.SchoolName,
                student.Provisions.Select(entry => $"{entry.Code}: {entry.Description}").ToList(),
                student.IsDeleted));

            foreach (AssessmentSubmission submission in student.Submissions)
            {
                submissions.Add(new(
                    submission.Id,
                    student.StudentId,
                    submission.SubmittedAt,
                    submission.SubmittedBy,
                    submission.SubmittedByEmail));
            }
        }

        foreach (AssessmentDownload download in assessment.Downloads)
        {
            List<AssessmentDetailsResponse.DownloadEvent> downloadEvents = [];

            foreach (AssessmentDownloadEvent downloadEvent in download.DownloadEvents)
            {
                downloadEvents.Add(new(
                    downloadEvent.DownloadedBy,
                    downloadEvent.DownloadedByEmail.ToString(),
                    downloadEvent.DownloadedAt));
            }

            downloads.Add(new(
                download.Id,
                download.Name,
                download.AvailableFrom,
                download.AvailableTo,
                download.IsRestricted,
                downloadEvents));
        }

        foreach (AssessmentInstruction instruction in assessment.Instructions)
        {
            instructions.Add(new(
                instruction.Id,
                instruction.Category,
                instruction.Details));
        }

        return new AssessmentDetailsResponse(
            assessment.Id,
            assessment.Name,
            assessment.CourseId,
            assessment.Course,
            assessment.Grade,
            assessment.DueDate,
            assessment.AvailableFrom,
            assessment.AvailableTo,
            assessment.IsLinkedToCanvas,
            students,
            submissions, 
            downloads,
            instructions);
    }
}
