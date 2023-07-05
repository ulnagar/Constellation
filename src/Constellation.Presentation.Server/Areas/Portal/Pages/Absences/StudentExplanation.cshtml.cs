namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Constellation.Application.Absences.CreateAbsenceResponseFromStudent;
using Constellation.Application.Absences.GetAbsenceForStudentResponse;
using Constellation.Application.Absences.ProvideStudentPartialAbsenceExplanation;
using Constellation.Core.Models.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

public class StudentExplanationModel : PageModel
{
    private readonly IMediator _mediator;

    public StudentExplanationModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public AbsenceForStudentResponse Absence { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    [BindProperty]
    public string Reason { get; set; }
    [BindProperty]
    public string StudentId { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        var absenceId = AbsenceId.FromValue(Id);

        var absenceRequest = await _mediator.Send(new GetAbsenceForStudentResponseQuery(absenceId), cancellationToken);

        if (absenceRequest.IsFailure)
            throw new InvalidDataException(absenceRequest.Error.Message);

        StudentId = absenceRequest.Value.StudentId;

        return Page();
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Reason))
            ModelState.AddModelError("Reason", "You must provide an explanation for this absence.");

        if (!ModelState.IsValid)
            return Page();

        var absenceId = AbsenceId.FromValue(Id);

        await _mediator.Send(new CreateAbsenceResponseFromStudentCommand(absenceId, StudentId, Reason), cancellationToken);

        return RedirectToPage("/Absences/Students", new { area = "Portal", studentId = Absence.StudentId });
    }
}
