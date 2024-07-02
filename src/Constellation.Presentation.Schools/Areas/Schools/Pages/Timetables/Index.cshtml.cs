namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Timetables;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.DTOs;
using Constellation.Application.Students.GetCurrentStudentsFromSchool;
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

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    public List<StudentDto> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<StudentDto>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (students.IsFailure)
        {
            ModalContent = new ErrorDisplay(students.Error);

            return;
        }

        Students = students.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();
    }
}