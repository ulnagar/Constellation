namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDetailsById;

using Abstractions.Messaging;
using Application.Models.Identity.Repositories;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetAssessmentDetailsByIdQueryHandler
: IQueryHandler<GetAssessmentDetailsByIdQuery, AssessmentDetailsResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetAssessmentDetailsByIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        IIdentityRepository identityRepository,
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _identityRepository = identityRepository;
        _contactRepository = contactRepository;
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
                string location = await GetDownloadLocation(downloadEvent, assessment, cancellationToken);

                downloadEvents.Add(new(
                    downloadEvent.DownloadedBy,
                    downloadEvent.DownloadedByEmail.ToString(),
                    location,
                    downloadEvent.DownloadedAt));
            }

            downloads.Add(new(
                download.Id,
                download.Name,
                download.AvailableFrom,
                download.AvailableTo,
                download.IsRestricted,
                download.IsDeleted,
                downloadEvents));
        }

        foreach (AssessmentInstruction instruction in assessment.Instructions)
        {
            instructions.Add(new(
                instruction.Id,
                instruction.Category,
                instruction.Details));
        }

        AssessmentDetailsResponse.CanvasLink? canvasLink = assessment.CanvasCourse is null
            ? null
            : new(
                assessment.CanvasCourse.Value,
                assessment.CanvasCourseName,
                assessment.CanvasAssignmentId.GetValueOrDefault(),
                assessment.CanvasAssignmentName,
                assessment.AllowedAttempts.GetValueOrDefault(),
                assessment.ForwardDate,
                assessment.CanvasAssessmentLink);

        return new AssessmentDetailsResponse(
            assessment.Id,
            assessment.Name,
            assessment.CourseId,
            assessment.Course,
            assessment.Grade,
            assessment.DueDate,
            assessment.AvailableFrom,
            assessment.AvailableTo,
            canvasLink,
            students,
            submissions, 
            downloads,
            instructions);
    }

    private async Task<string> GetDownloadLocation(
        AssessmentDownloadEvent downloadEvent,
        Assessment assessment,
        CancellationToken cancellationToken)
    {
        AppUser? user = await _identityRepository.GetUser(downloadEvent.UserId, cancellationToken);

        if (user is null || !user.IsSchoolContact)
            return string.Empty;

        AppUserLink? link = user.Links.FirstOrDefault(link => !link.IsDeleted && link.Type == LinkType.Contact);

        if (link is null)
            return string.Empty;

        SchoolContactId contactId = SchoolContactId.FromValue(link.LinkId);
        SchoolContact? contact = await _contactRepository.GetById(contactId, cancellationToken);

        if (contact is null)
            return string.Empty;

        List<SchoolCode> linkedSchoolCodes = assessment.Students.Select(entry => entry.SchoolCode).ToList();

        foreach (SchoolContactRole assignment in contact.Assignments.Where(entry => !entry.IsDeleted))
        {
            if (linkedSchoolCodes.Contains(assignment.SchoolCode))
                return assignment.SchoolName;
        }

        return string.Empty;
    }
}
