namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetCurrentStudentProvisionsByStudentId;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCurrentStudentProvisionsByStudentIdQueryHandler
    : IQueryHandler<GetCurrentStudentProvisionsByStudentIdQuery, List<AssessmentProvisionResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetCurrentStudentProvisionsByStudentIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger;
    }

    public async Task<Result<List<AssessmentProvisionResponse>>> Handle(GetCurrentStudentProvisionsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        List<AssessmentProvisionResponse> response = [];

        List<Provision> provisions = await _assessmentRepository.GetCurrentProvisionsForStudent(request.StudentId, cancellationToken);

        foreach (Provision provision in provisions)
            response.Add(new(provision.Id, provision.Code, provision.Description));

        if (request.AssessmentId is not null)
        {
            Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId.Value, cancellationToken);

            if (assessment is null)
                return response;

            AssessmentStudent? student = assessment.Students.FirstOrDefault(entry => entry.StudentId == request.StudentId);

            if (student is null)
                return response;

            List<ProvisionId> studentProvisions = student.Provisions.Select(entry => entry.ProvisionId).ToList();

            if (studentProvisions.Count == 0)
                return response;

            provisions = await _assessmentRepository.GetProvisionsFromList(studentProvisions, cancellationToken);

            foreach (Provision provision in provisions)
            {
                if (response.Any(entry => entry.Id == provision.Id))
                    continue;

                response.Add(new(provision.Id, provision.Code, provision.Description));
            }
        }

        return response;
    }
}
