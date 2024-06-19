namespace Constellation.Application.WorkFlows.GetCaseSummaryList;

using Abstractions.Messaging;
using Awards.GetAwardCountsByTypeByGrade;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCaseSummaryListQueryHandler
: IQueryHandler<GetCaseSummaryListQuery, List<CaseSummaryResponse>>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetCaseSummaryListQueryHandler(
        ICaseRepository caseRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetCaseSummaryListQuery>();
    }

    public async Task<Result<List<CaseSummaryResponse>>> Handle(GetCaseSummaryListQuery request, CancellationToken cancellationToken)
    {
        List<CaseSummaryResponse> responses = new();

        List<Case> items = await _caseRepository.GetAll(cancellationToken);

        foreach (Case item in items)
        {
            if (!request.IsAdmin && item.Actions.All(action => action.AssignedToId != request.CurrentUserId))
                continue;

            string studentId = string.Empty;
            string description = string.Empty;

            if (item.Type!.Equals(CaseType.Attendance))
            {
                AttendanceCaseDetail details = item.Detail as AttendanceCaseDetail;

                studentId = details.StudentId;
                description = $"Attendance Case for {details.PeriodLabel} - {details.Severity.Value}";
            }

            if (item.Type!.Equals(CaseType.Compliance))
            {
                ComplianceCaseDetail details = item.Detail as ComplianceCaseDetail;

                studentId = details.StudentId;
                description = $"Compliance Case for {details.IncidentType} - {details.Subject}";
            }

            Student student = await _studentRepository.GetById(studentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(studentId), true)
                    .Warning("Could not generate list of Case Summary");

                return Result.Failure<List<CaseSummaryResponse>>(StudentErrors.NotFound(studentId));
            }

            Name name = student.GetName();

            responses.Add(new(
                item.Id,
                name,
                description,
                item.Status,
                item.CreatedAt,
                item.DueDate,
                item.Actions.Count,
                item.Actions.Count(action => action.Status == ActionStatus.Open)));
        }

        return responses;
    }
}
