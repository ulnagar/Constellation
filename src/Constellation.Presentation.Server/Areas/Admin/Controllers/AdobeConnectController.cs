using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Admin.Models;
using Constellation.Presentation.Server.Areas.Admin.Models.AdobeConnect;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AuthRoles.Admin)]
    public class AdobeConnectController : BaseController
    {
        private readonly IAdobeConnectService _adobeConnectService;

        public AdobeConnectController(IUnitOfWork unitOfWork, IAdobeConnectService adobeConnectService)
            : base(unitOfWork)
        {
            _adobeConnectService = adobeConnectService;
        }

        public async Task<IActionResult> _getUserACPID(string username)
        {
            var acpid = await _adobeConnectService.GetUserPrincipalId(username);

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
            var viewModel = await CreateViewModel<ActionsViewModel>();

            return View(viewModel);
        }

        public async Task<IActionResult> GetNewRooms(string scoId)
        {
            var viewModel = await CreateViewModel<ActionsViewModel>();
            viewModel.Rooms = await _adobeConnectService.UpdateRooms("3229451");

            return View("NewRooms", viewModel);
        }

        public async Task<IActionResult> UpdateUsers()
        {
            var viewModel = await CreateViewModel<ActionsViewModel>();
            viewModel.Users = await _adobeConnectService.UpdateUsers();

            return View("NewUsers", viewModel);
        }
    }
}