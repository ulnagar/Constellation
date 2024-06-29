﻿namespace Constellation.Presentation.Schools.Areas.Schools.Pages;

using Application.DTOs;
using Application.Students.GetCurrentStudentsFromSchool;
using Constellation.Application.Models.Auth;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DashboardModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory scopeFactory)
        : base(httpContextAccessor, scopeFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public List<StudentDto> Students { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        if (string.IsNullOrWhiteSpace(CurrentSchoolCode))
        {
            return Page();
        }

        Result<List<StudentDto>> studentsRequest = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentsRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        return Page();
    }
}
