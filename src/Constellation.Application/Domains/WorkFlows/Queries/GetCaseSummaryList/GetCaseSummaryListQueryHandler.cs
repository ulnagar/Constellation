namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseSummaryList;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
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
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetCaseSummaryListQueryHandler(
        ICaseRepository caseRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
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

            StudentId studentId = StudentId.Empty;
            string description = string.Empty;

            if (item.Type!.Equals(CaseType.Training))
            {
                TrainingCaseDetail details = item.Detail as TrainingCaseDetail;

                StaffMember staffMember = await _staffRepository.GetById(details.StaffId, cancellationToken);

                if (staffMember is null)
                {
                    _logger
                        .ForContext(nameof(Case), item, true)
                        .ForContext(nameof(Error), StaffMemberErrors.NotFound(details.StaffId), true)
                        .Warning("Could not generate list of Case Summary");

                    return Result.Failure<List<CaseSummaryResponse>>(StaffMemberErrors.NotFound(details.StaffId));
                }

                description = $"Training Case for {details.ModuleName} - {details.DueDate:d}";
                
                responses.Add(new(
                    item.Id,
                    staffMember.Name,
                    description,
                    item.Status,
                    item.CreatedAt,
                    item.DueDate,
                    item.Actions.Count,
                    item.Actions.Count(action => action.Status == ActionStatus.Open)));

                continue;
            }

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
            
            responses.Add(new(
                item.Id,
                student.Name,
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
