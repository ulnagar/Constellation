namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsByStudentId;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCurrentAssessmentsByStudentIdQueryHandler
: IQueryHandler<GetCurrentAssessmentsByStudentIdQuery, List<AssessmentDetailsResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetCurrentAssessmentsByStudentIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetCurrentAssessmentsByStudentIdQuery>();
    }

    public async Task<Result<List<AssessmentDetailsResponse>>> Handle(GetCurrentAssessmentsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        List<AssessmentDetailsResponse> responses = [];

        List<Assessment> assessments = await _assessmentRepository.GetCurrentAssessmentsForStudent(request.StudentId, cancellationToken);

        foreach (Assessment assessment in assessments)
        {
            List<AssessmentDetailsResponse.Student> students = [];
            List<AssessmentDetailsResponse.Download> downloads = [];
            List<AssessmentDetailsResponse.Submission> submissions = [];
            List<AssessmentDetailsResponse.Instruction> instructions = [];

            foreach (AssessmentStudent student in assessment.Students.Where(student => student.StudentId == request.StudentId))
            {
                students.Add(new(
                    student.StudentId,
                    student.Student,
                    student.StudentGrade,
                    student.SchoolCode,
                    student.SchoolName,
                    student.Provisions.Select(entry => $"{entry.Code}: {entry.Description}").ToList(),
                    student.IsDeleted));
            }

            foreach (AssessmentDownload download in assessment.Downloads)
            {
                List<AssessmentDetailsResponse.DownloadEvent> downloadEvents = [];

                if (!download.IsAvailable(_dateTime.Today))
                    continue;

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

            responses.Add(new(
                assessment.Id,
                assessment.Name,
                assessment.CourseId,
                assessment.Course,
                assessment.Grade,
                assessment.DueDate,
                assessment.AvailableFrom,
                assessment.AvailableTo,
                null,
                students,
                submissions,
                downloads,
                instructions));
        }

        return responses;
    }
}
