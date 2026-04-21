namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisions;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Core.Enums;
using Core.Models.Assessments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetStudentProvisionsQueryHandler
: IQueryHandler<GetStudentProvisionsQuery, List<StudentProvisionResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentProvisionsQueryHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentProvisionResponse>>> Handle(GetStudentProvisionsQuery request, CancellationToken cancellationToken)
    {
        List<StudentProvisionResponse> response = [];

        List<StudentProvision> studentProvisions = await _assessmentRepository.GetStudentProvisions(cancellationToken);

        List<StudentId> studentIds = studentProvisions.Select(entry => entry.StudentId).ToList();

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        foreach (StudentProvision provision in studentProvisions)
        {
            Student? student = students.FirstOrDefault(entry => entry.Id == provision.StudentId);

            response.Add(new(
                provision.Id,
                provision.ProvisionCode,
                provision.ProvisionDescription,
                provision.Student,
                student?.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                provision.Year,
                provision.IsDeleted));
        }

        return response;
    }
}
