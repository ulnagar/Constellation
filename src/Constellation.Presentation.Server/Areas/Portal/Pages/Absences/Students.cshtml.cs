namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Application.Students.Models;
using Constellation.Application.Absences.GetAbsenceSummaryForStudent;
using Constellation.Application.Students.GetStudentById;
using Core.Models.Students.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.Helpers.ModelBinders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[AllowAnonymous]
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
    [ModelBinder(typeof(ConstructorBinder))]
    public StudentId StudentId { get; set; } = StudentId.Empty;
    public string StudentName { get; set; }

    public List<StudentAbsenceSummaryResponse> Absences { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        if (StudentId == StudentId.Empty)
            throw new ArgumentOutOfRangeException(nameof(StudentId));

        // Get Student details
        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(StudentId), cancellationToken);

        if (studentRequest.IsFailure)
            throw new InvalidDataException(studentRequest.Error.Message);

        StudentName = studentRequest.Value.Name.DisplayName;

        // Get all absences for this student from this year.
        Result<List<StudentAbsenceSummaryResponse>> absencesRequest = ShowAll switch
        {
            true => await _mediator.Send(new GetAbsenceSummaryForStudentQuery(StudentId, false), cancellationToken),
            false => await _mediator.Send(new GetAbsenceSummaryForStudentQuery(StudentId, true), cancellationToken)
        };

        if (absencesRequest.IsFailure)
            throw new InvalidDataException(absencesRequest.Error.Message);

        Absences = absencesRequest.Value;

        return Page();
    }
}
