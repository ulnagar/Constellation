namespace Constellation.Application.Domains.Assessments.Assessments.Models;

using Core.Enums;
using Core.Models.Assessments.Enums;
using Core.Models.Assessments.Identifiers;
using Core.Models.Canvas.Models;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.ValueObjects;

public sealed record AssessmentDetailsResponse(
    AssessmentId Id,
    string Name,
    CourseId CourseId,
    string Course,
    Grade Grade,
    DateTimeOffset DueDate,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableTo,
    AssessmentDetailsResponse.CanvasLink? CanvasDetails,
    List<AssessmentDetailsResponse.Student> Students,
    List<AssessmentDetailsResponse.Submission> Submissions,
    List<AssessmentDetailsResponse.Download> Downloads,
    List<AssessmentDetailsResponse.Instruction> Instructions)
{
    public sealed record Student(
        StudentId StudentId,
        Name StudentName,
        Grade StudentGrade,
        SchoolCode SchoolCode,
        string SchoolName,
        List<string> Provisions,
        bool IsDeleted);

    public sealed record CanvasLink(
        CanvasCourseCode CourseCode,
        string CourseName,
        int AssignmentId,
        string AssignmentName,
        int AllowedAttempts,
        DateTimeOffset? ForwardDate,
        Uri AssessmentLink);

    public sealed record Submission(
        SubmissionId SubmissionId,
        StudentId StudentId,
        DateTimeOffset SubmittedAt,
        string SubmittedBy,
        EmailAddress SubmittedByEmail);

    public sealed record Download(
        AssessmentDownloadId DownloadId,
        string Name,
        DateOnly AvailableFrom,
        DateOnly AvailableTo,
        bool IsRestricted,
        List<DownloadEvent> DownloadEvents);

    public sealed record DownloadEvent(
        string User,
        string Email,
        DateTimeOffset DownloadedOn);

    public sealed record Instruction(
        AssessmentInstructionId InstructionId,
        UserCategory Category,
        string Description);
}