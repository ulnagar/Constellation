namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisionById;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetStudentProvisionByIdQueryHandler
: IQueryHandler<GetStudentProvisionByIdQuery, StudentProvisionResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetStudentProvisionByIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
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

        return new StudentProvisionResponse(
                provision.Id,
                provision.ProvisionCode,
                provision.ProvisionDescription,
                provision.Student,
                provision.Year,
                provision.IsDeleted);
    }
}