namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Absences.GetOutstandingAbsencesForSchool;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
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
        Result<List<OutstandingAbsencesForSchoolResponse>> absencesRequest = await _mediator.Send(new GetOutstandingAbsencesForSchoolQuery(CurrentSchoolCode));

        if (absencesRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(absencesRequest.Error);

            return;
        }

        Absences = absencesRequest.Value;

        UnexplainedPartialsCount = Absences.Count(absence => absence.AbsenceTimeframe != absence.PeriodTimeframe && absence.AbsenceResponseId == null);
        UnverifiedPartialsCount = Absences.Count(absence => absence.AbsenceTimeframe != absence.PeriodTimeframe && absence.AbsenceResponseId != null);
        UnexplainedWholesCount = Absences.Count(absence => absence.AbsenceTimeframe == absence.PeriodTimeframe);

        Absences = Type switch
        {
            AbsenceCategory.UnexplainedPartials => Absences.Where(absence =>
                    absence.AbsenceTimeframe != absence.PeriodTimeframe &&
                    absence.AbsenceResponseId == null)
                .ToList(),
            AbsenceCategory.UnexplainedWholes => Absences.Where(absence =>
                    absence.AbsenceTimeframe == absence.PeriodTimeframe)
                .ToList(),
            AbsenceCategory.UnverifiedPartials => Absences.Where(absence =>
                    absence.AbsenceTimeframe != absence.PeriodTimeframe &&
                    absence.AbsenceResponseId != null)
                .ToList()
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