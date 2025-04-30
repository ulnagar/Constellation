namespace Constellation.Presentation.Students.Areas.Students.Pages.Attendance;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForStudent;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.IsStudent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Attendance;

    public List<AbsenceForStudentResponse> Absences { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public AbsenceCategory AbsencesType { get; set; } = AbsenceCategory.UnexplainedPartials;

    public int ExplainedWholesCount { get; set; }
    public int VerifiedPartialsCount { get; set; }
    public int UnexplainedPartialsCount { get; set; }
    public int UnverifiedPartialsCount { get; set; }
    public int UnexplainedWholesCount { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve absence data by user {user} of type {type}", _currentUserService.UserName, AbsencesType);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve absence data by user {user} of type {type}", _currentUserService.UserName, AbsencesType);

            ModalContent = new ErrorDisplay(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<List<AbsenceForStudentResponse>> absencesRequest = await _mediator.Send(new GetAbsencesForStudentQuery(studentId));

        if (absencesRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), absencesRequest.Error, true)
                .Warning("Failed to retrieve absence data by user {user} of type {type}", _currentUserService.UserName, AbsencesType);

            ModalContent = new ErrorDisplay(absencesRequest.Error);

            return;
        }

        Absences = absencesRequest.Value;

        VerifiedPartialsCount = Absences.Count(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.VerifiedPartial);
        ExplainedWholesCount = Absences.Count(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.ExplainedWhole);
        UnexplainedPartialsCount = Absences.Count(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.UnexplainedPartial);
        UnverifiedPartialsCount = Absences.Count(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.UnverifiedPartial);
        UnexplainedWholesCount = Absences.Count(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.UnexplainedWhole);

        Absences = AbsencesType switch
        {
            AbsenceCategory.UnexplainedPartials => Absences
                .Where(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.UnexplainedPartial)
                .ToList(),
            AbsenceCategory.UnexplainedWholes => Absences
                .Where(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.UnexplainedWhole)
                .ToList(),
            AbsenceCategory.UnverifiedPartials => Absences
                .Where(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.UnverifiedPartial)
                .ToList(),
            AbsenceCategory.ExplainedWholes => Absences
                .Where(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.ExplainedWhole)
                .ToList(),
            AbsenceCategory.VerifiedPartials => Absences
                .Where(absence => absence.Status == AbsenceForStudentResponse.AbsenceStatus.VerifiedPartial)
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