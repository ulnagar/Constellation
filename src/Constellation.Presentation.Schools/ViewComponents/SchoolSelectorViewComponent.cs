namespace Constellation.Presentation.Schools.ViewComponents;

using Application.DTOs;
using Application.Models.Auth;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Schools.GetSchoolsFromList;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.SchoolSelector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class SchoolSelectorViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public SchoolSelectorViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> Invoke()
    {
        SchoolSelectorViewModel viewModel = new();

        List<string> schoolCodes = User.IsInRole(AuthRoles.Admin)
            ? (await _mediator.Send(new GetAllPartnerSchoolCodesQuery())).ToList()
            : new List<string>(); // TODO: R1.15: Find school codes for linked schools of user

        Result<List<SchoolDto>> schoolsRequest = await _mediator.Send(new GetSchoolsFromListQuery(schoolCodes));

        if (schoolsRequest.IsFailure)
            return Content(string.Empty);

        viewModel.ValidSchools = schoolsRequest.Value;

        viewModel.CurrentSchoolCode = schoolsRequest.Value.OrderBy(school => school.Code).First().Code;

        return View("SchoolSelector", viewModel);
    }
}
