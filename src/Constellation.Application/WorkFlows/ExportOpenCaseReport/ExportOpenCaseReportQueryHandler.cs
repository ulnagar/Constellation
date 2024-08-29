namespace Constellation.Application.WorkFlows.ExportOpenCaseReport;

using Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Extensions;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Action = Core.Models.WorkFlow.Action;

internal sealed class ExportOpenCaseReportQueryHandler
: IQueryHandler<ExportOpenCaseReportQuery, FileDto>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IExcelService _excelService;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    private int Index { get; set; }

    public ExportOpenCaseReportQueryHandler(
        ICaseRepository caseRepository,
        IExcelService excelService,
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _excelService = excelService;
        _studentRepository = studentRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<ExportOpenCaseReportQuery>();
    }

    public async Task<Result<FileDto>> Handle(ExportOpenCaseReportQuery request, CancellationToken cancellationToken)
    {
        List<CaseReportItem> reportItems = new();

        List<Case> cases = await _caseRepository.GetAllCurrent(cancellationToken);

        foreach (Case item in cases)
        {
            string shortCaseId = item.Id.Value.ToShortString();

            if (item.Type!.Equals(CaseType.Attendance))
            {
                AttendanceCaseDetail details = item.Detail as AttendanceCaseDetail;

                Student student = await _studentRepository.GetBySRN(details!.StudentId, cancellationToken);

                if (student is null)
                {
                    _logger
                        .ForContext(nameof(Case), item, true)
                        .ForContext(nameof(Error), StudentErrors.NotFound(details.StudentId), true)
                        .Warning("Could not export open Cases");

                    return Result.Failure<FileDto>(StudentErrors.NotFound(details.StudentId));
                }

                Name name = student.GetName();

                reportItems.Add(new(
                    $"{shortCaseId}.00",
                    student.GetName(),
                    student.CurrentGrade,
                    item.ToString(),
                    DateOnly.FromDateTime(item.CreatedAt),
                    null,
                    null,
                    _dateTime.Today.DayOfYear - item.CreatedAt.DayOfYear));


                List<Action> parentActions = item.Actions
                    .Where(entry => entry.ParentActionId is null)
                    .OrderBy(entry => entry.CreatedAt)
                    .ToList();

                Index = 0;

                foreach (Action action in parentActions)
                    CreateFromAction(reportItems, item.Actions.ToList(), shortCaseId, action, student.GetName(), student.CurrentGrade);
            }
        }

        MemoryStream stream = await _excelService.CreateWorkFlowReport(reportItems, cancellationToken);

        // Wrap data in return object
        FileDto reportDto = new()
        {
            FileData = stream.ToArray(),
            FileName = $"WorkFlow Cases Report - {_dateTime.Today:O}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        // Return for download
        return reportDto;
    }

    private void CreateFromAction(List<CaseReportItem> reportItems, List<Action> actions, string shortCaseId, Action action, Name student, Grade grade)
    {
        Index++;

        string actionIndex = Index.ToString().PadLeft(2, '0');

        DateOnly? completedDate = action.Status switch
        {
            not null when action.Status.Equals(ActionStatus.Open) => null,
            not null when action.Status.Equals(ActionStatus.Completed) => action.ModifiedAt == DateTime.MinValue ? DateOnly.FromDateTime(action.CreatedAt) : DateOnly.FromDateTime(action.ModifiedAt),
            not null when action.Status.Equals(ActionStatus.Cancelled) => action.ModifiedAt == DateTime.MinValue ? DateOnly.FromDateTime(action.CreatedAt) : DateOnly.FromDateTime(action.ModifiedAt),
            _ => null
        };

        reportItems.Add(new(
            $"{shortCaseId}.{actionIndex}",
            student,
            grade,
            action.ToString(),
            DateOnly.FromDateTime(action.CreatedAt),
            completedDate,
            action.AssignedTo,
            _dateTime.Today.DayOfYear - action.CreatedAt.DayOfYear));

        foreach (Action childAction in actions.Where(entry => entry.ParentActionId == action.Id))
            CreateFromAction(reportItems, actions, shortCaseId, childAction, student, grade);
    }
}