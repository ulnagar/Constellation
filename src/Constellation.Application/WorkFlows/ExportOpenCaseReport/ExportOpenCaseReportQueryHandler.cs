namespace Constellation.Application.WorkFlows.ExportOpenCaseReport;

using Abstractions.Messaging;
using Constellation.Application.Helpers;
using Core.Abstractions.Clock;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using DTOs;
using Extensions;
using Interfaces.Services;
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
    private readonly IDateTimeProvider _dateTime;

    private int Index { get; set; }

    public ExportOpenCaseReportQueryHandler(
        ICaseRepository caseRepository,
        IExcelService excelService,
        IDateTimeProvider dateTime)
    {
        _caseRepository = caseRepository;
        _excelService = excelService;
        _dateTime = dateTime;
    }

    public async Task<Result<FileDto>> Handle(ExportOpenCaseReportQuery request, CancellationToken cancellationToken)
    {
        List<CaseReportItem> reportItems = new();

        List<Case> cases = await _caseRepository.GetAllCurrent(cancellationToken);

        foreach (Case item in cases)
        {
            string shortCaseId = item.Id.Value.ToShortString();

            reportItems.Add(new(
                $"{shortCaseId}.00",
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
                CreateFromAction(reportItems, item.Actions.ToList(), shortCaseId, action);
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

    private void CreateFromAction(List<CaseReportItem> reportItems, List<Action> actions, string shortCaseId, Action action)
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
            action.ToString(),
            DateOnly.FromDateTime(action.CreatedAt),
            completedDate,
            action.AssignedTo,
            _dateTime.Today.DayOfYear - action.CreatedAt.DayOfYear));

        foreach (Action childAction in actions.Where(entry => entry.ParentActionId == action.Id))
            CreateFromAction(reportItems, actions, shortCaseId, childAction);
    }
}