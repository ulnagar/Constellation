namespace Constellation.Presentation.Server.Areas.Partner.Controllers;
using Constellation.Application.Features.API.Schools.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using Core.Models.SchoolContacts.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Partner")]
[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
public class SchoolsController : Controller
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISchoolService _schoolService;
    private readonly IMediator _mediator;

    public SchoolsController(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ISchoolService schoolService,
        IMediator mediator)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _schoolService = schoolService;
        _mediator = mediator;
    }
    
    public async Task<IActionResult> _GetGraphData(string id, int day)
    {
        var data = await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });

        return Json(data);
    }
    
    [Authorize]
    [HttpPost("/Partner/Schools/Map")]
    public async Task<IActionResult> ViewMap(IList<string> schoolCodes)
    {
        var vm = new School_MapViewModel();

        vm.Layers = _unitOfWork.Schools.GetForMapping(schoolCodes);
        vm.PageHeading = "Map of Schools";

        return View("Map", vm);
    }

    [AllowAnonymous]
    [HttpGet("/Partner/Schools/Map")]
    public async Task<IActionResult> ViewAnonMap()
    {
        School_MapViewModel vm = new School_MapViewModel();

        vm.Layers = _unitOfWork.Schools.GetForMapping(new List<string>());
        vm.PageHeading = "Map of Schools";

        return View("Map", vm);
    }
}