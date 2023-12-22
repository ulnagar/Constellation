namespace Constellation.Presentation.Server.Areas.Admin.Controllers;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Admin.Models.AdobeConnect;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

[Area("Admin")]
[Authorize(Roles = AuthRoles.Admin)]
public class AdobeConnectController : Controller
{
    private readonly IAdobeConnectService _adobeConnectService;

    public AdobeConnectController(
        IUnitOfWork unitOfWork, 
        IAdobeConnectService adobeConnectService,
        IMediator mediator)
    {
        _adobeConnectService = adobeConnectService;
    }

    public async Task<IActionResult> _getUserACPID(string username)
    {
        string acpid = await _adobeConnectService.GetUserPrincipalId(username);

        if (acpid == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return Json(new { });
        }
        else
        {
            return Json(acpid);
        }
    }

    public async Task<IActionResult> Actions()
    {
        ActionsViewModel viewModel = new();

        return View(viewModel);
    }

    public async Task<IActionResult> GetNewRooms(string scoId)
    {
        ActionsViewModel viewModel = new();
        viewModel.Rooms = await _adobeConnectService.UpdateRooms("3229451");

        return View("NewRooms", viewModel);
    }

    public async Task<IActionResult> UpdateUsers()
    {
        ActionsViewModel viewModel = new();
        viewModel.Users = await _adobeConnectService.UpdateUsers();

        return View("NewUsers", viewModel);
    }
}