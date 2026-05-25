namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsBySchoolCode;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCurrentAssessmentsBySchoolCodeQueryHandler
: IQueryHandler<GetCurrentAssessmentsBySchoolCodeQuery, List<AssessmentDetailsResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetCurrentAssessmentsBySchoolCodeQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger
            .ForContext<GetCurrentAssessmentsBySchoolCodeQuery>();
    }

    public async Task<Result<List<AssessmentDetailsResponse>>> Handle(GetCurrentAssessmentsBySchoolCodeQuery request, CancellationToken cancellationToken)
    {
        List<AssessmentDetailsResponse> responses = [];

        List<Assessment> assessments = await _assessmentRepository.GetCurrentAssessmentsForSchoolCode(request.SchoolCode, cancellationToken);

        foreach (Assessment assessment in assessments)
        {
            List<AssessmentDetailsResponse.Student> students = [];
            List<AssessmentDetailsResponse.Download> downloads = [];
            List<AssessmentDetailsResponse.Submission> submissions = [];
            List<AssessmentDetailsResponse.Instruction> instructions = [];

            foreach (AssessmentStudent student in assessment.Students.Where(student => student.SchoolCode == request.SchoolCode))
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
