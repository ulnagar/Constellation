namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Attendance;

using Application.Absences.GetAbsencesForFamily;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
public class AbsencesModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AbsencesModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AbsencesModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Attendance;

    public List<AbsenceForFamilyResponse> Absences { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public AbsenceCategory AbsencesType { get; set; } = AbsenceCategory.UnexplainedWholes;

    public int ExplainedWholesCount { get; set; } = 0;
    public int VerifiedPartialsCount { get; set; } = 0;
    public int UnexplainedPartialsCount { get; set; } = 0;
    public int UnverifiedPartialsCount { get; set; } = 0;
    public int UnexplainedWholesCount { get; set; } = 0;

    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve absence data by user {user} of type {type}", _currentUserService.UserName, AbsencesType);

        Result<List<AbsenceForFamilyResponse>> absencesRequest = await _mediator.Send(new GetAbsencesForFamilyQuery(_currentUserService.EmailAddress));

        if (absencesRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(absencesRequest.Error);

            return;
        }

        Absences = absencesRequest.Value;

        VerifiedPartialsCount = Absences.Count(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.VerifiedPartial);
        ExplainedWholesCount = Absences.Count(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.ExplainedWhole);
        UnexplainedPartialsCount = Absences.Count(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.UnexplainedPartial);
        UnverifiedPartialsCount = Absences.Count(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.UnverifiedPartial);
        UnexplainedWholesCount = Absences.Count(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.UnexplainedWhole);

        Absences = AbsencesType switch
        {
            AbsenceCategory.UnexplainedPartials => Absences
                .Where(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.UnexplainedPartial)
                .ToList(),
            AbsenceCategory.UnexplainedWholes => Absences
                .Where(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.UnexplainedWhole)
                .ToList(),
            AbsenceCategory.UnverifiedPartials => Absences
                .Where(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.UnverifiedPartial)
                .ToList(),
            AbsenceCategory.ExplainedWholes => Absences
                .Where(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.ExplainedWhole)
                .ToList(),
            AbsenceCategory.VerifiedPartials => Absences
                .Where(absence => absence.Status == AbsenceForFamilyResponse.AbsenceStatus.VerifiedPartial)
                .ToList()
        };

        Absences = Absences
            .OrderBy(absence => absence.AbsenceDate)
            .ThenBy(absence => absence.StudentGrade)
            .ToList();
    }

    public enum AbsenceCategory
    {
        UnexplainedPartials,
        UnverifiedPartials,
        UnexplainedWholes,
        ExplainedWholes,
        VerifiedPartials
    }
}