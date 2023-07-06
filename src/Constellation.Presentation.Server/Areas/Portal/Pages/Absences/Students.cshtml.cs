namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Constellation.Application.Absences.GetAbsenceSummaryForStudent;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentsModel : PageModel
{
    private readonly IMediator _mediator;

    public StudentsModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public bool ShowAll { get; set; }

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }
    public string StudentName { get; set; }

    public List<StudentAbsenceSummaryResponse> Absences { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(StudentId))
            throw new ArgumentOutOfRangeException(nameof(StudentId));

        // Get all absences for this student from this year.
        var absencesRequest = await _mediator.Send(new GetAbsenceSummaryForStudentQuery(StudentId), cancellationToken);

        if (absencesRequest.IsFailure)
            throw new InvalidDataException(absencesRequest.Error.Message);

        Absences = absencesRequest.Value;

        if (!ShowAll)
            Absences = Absences
                .Where(entry => !entry.IsExplained)
                .ToList();

        return Page();
    }
}
