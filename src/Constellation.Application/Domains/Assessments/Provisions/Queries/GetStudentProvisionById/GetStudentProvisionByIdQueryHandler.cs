namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisionById;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Core.Enums;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetStudentProvisionByIdQueryHandler
: IQueryHandler<GetStudentProvisionByIdQuery, StudentProvisionResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentProvisionByIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetStudentProvisionByIdQuery>();
    }

    public async Task<Result<StudentProvisionResponse>> Handle(GetStudentProvisionByIdQuery request, CancellationToken cancellationToken)
    {
        StudentProvision? provision = await _assessmentRepository.GetStudentProvisionById(request.Id, cancellationToken);

        if (provision is null)
        {
            _logger
                .ForContext(nameof(GetStudentProvisionByIdQuery), request, true)
                .ForContext(nameof(Error), StudentProvisionErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Student Provision");

            return Result.Failure<StudentProvisionResponse>(StudentProvisionErrors.NotFound(request.Id));
        }

        Student? student = await _studentRepository.GetById(provision.StudentId, cancellationToken);

        return new StudentProvisionResponse(
                provision.Id,
                provision.ProvisionCode,
                provision.ProvisionDescription,
                provision.Student,
                student?.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                provision.Year,
                provision.IsDeleted);
    }
}