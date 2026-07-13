namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Absences.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Absences_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        LinkGenerator linkGenerator,
        ILogger logger,
        ICurrentUserService currentUserService)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForSchoolPortal();
    }
    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    public int UnexplainedPartialsCount { get; set; }
    public int UnverifiedPartialsCount { get; set; }
    public int UnexplainedWholesCount { get; set; }

    public List<OutstandingAbsencesForSchoolResponse> Absences { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public AbsenceCategory Type { get; set; } = AbsenceCategory.UnverifiedPartials;

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve absence data by user {user} of type {type}", _currentUserService.UserName, Type);
        
        Result<List<OutstandingAbsencesForSchoolResponse>> absencesRequest = await _mediator.Send(new GetOutstandingAbsencesForSchoolQuery(CurrentSchoolCode!));

        if (absencesRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(absencesRequest.Error);

            return;
        }

        Absences = absencesRequest.Value;

        UnexplainedPartialsCount = Absences.Count(absence => absence.AbsenceTimeframe != absence.PeriodTimeframe && absence.AbsenceResponseId == AbsenceResponseId.Empty);
        UnverifiedPartialsCount = Absences.Count(absence => absence.AbsenceTimeframe != absence.PeriodTimeframe && absence.AbsenceResponseId != AbsenceResponseId.Empty);
        UnexplainedWholesCount = Absences.Count(absence => absence.AbsenceTimeframe == absence.PeriodTimeframe);

        Absences = Type switch
        {
            AbsenceCategory.UnexplainedPartials => Absences.Where(absence =>
                    absence.AbsenceTimeframe != absence.PeriodTimeframe &&
                    absence.AbsenceResponseId == AbsenceResponseId.Empty)
                .ToList(),
            AbsenceCategory.UnexplainedWholes => Absences.Where(absence =>
                    absence.AbsenceTimeframe == absence.PeriodTimeframe)
                .ToList(),
            AbsenceCategory.UnverifiedPartials => Absences.Where(absence =>
                    absence.AbsenceTimeframe != absence.PeriodTimeframe &&
                    absence.AbsenceResponseId != AbsenceResponseId.Empty)
                .ToList(),
            _ => Absences
        };

        Absences = Absences
            .OrderBy(absence => absence.StudentGrade)
            .ThenBy(absence => absence.StudentName)
            .ToList();
    }

    public enum AbsenceCategory
    {
        UnexplainedPartials,
        UnverifiedPartials,
        UnexplainedWholes
    }
}