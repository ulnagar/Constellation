namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetStudentsFromAssessment;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assessments.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Serilog;
using System.Collections.Generic;

internal sealed class GetStudentsFromAssessmentQueryHandler
    : IQueryHandler<GetStudentsFromAssessmentQuery, List<Student>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentsFromAssessmentQueryHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetStudentsFromAssessmentQuery>();
    }

    public async Task<Result<List<Student>>> Handle(GetStudentsFromAssessmentQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetStudentsFromAssessmentQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to retrieve list of students linked to Assessment");

            return Result.Failure<List<Student>>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<StudentId> studentIds = assessment.Students
            .Where(student => !student.IsDeleted)
            .Select(student => student.StudentId)
            .ToList();

        return await _studentRepository.GetListFromIds(studentIds, cancellationToken);
    }
}
