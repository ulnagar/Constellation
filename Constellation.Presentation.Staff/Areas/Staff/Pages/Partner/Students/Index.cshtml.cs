namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Students.GetFilteredStudents;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public sealed class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;

    [BindProperty(SupportsGet = true)] 
    public StudentFilter Filter { get; set; } = StudentFilter.Active;

    public List<FilteredStudentResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<FilteredStudentResponse>>? students = await _mediator.Send(new GetFilteredStudentsQuery(Filter));

        if (students.IsFailure)
        {
            Error = new()
            {
                Error = students.Error,
                RedirectPath = null
            };
        }

        Students = students.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.StudentName.SortOrder)
            .ToList();
    }
}
