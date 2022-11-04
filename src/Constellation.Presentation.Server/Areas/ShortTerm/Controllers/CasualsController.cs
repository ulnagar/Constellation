using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.ShortTerm.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Controllers
{
    [Area("ShortTerm")]
    [Roles(AuthRoles.Admin, AuthRoles.CoverEditor, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class CasualsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICasualService _casualService;

        public CasualsController(IUnitOfWork unitOfWork, ICasualService casualService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _casualService = casualService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> All()
        {
            var casuals = await _unitOfWork.Casuals.ForListAsync(casual => true);

            var viewModel = await CreateViewModel<Casual_ViewModel>();
            viewModel.Casuals = casuals.Select(Casual_ViewModel.CasualDto.ConvertFromCasual).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var casuals = await _unitOfWork.Casuals.ForListAsync(casual => casual.IsDeleted);

            var viewModel = await CreateViewModel<Casual_ViewModel>();
            viewModel.Casuals = casuals.Select(Casual_ViewModel.CasualDto.ConvertFromCasual).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Active()
        {
            var casuals = await _unitOfWork.Casuals.ForListAsync(casual => !casual.IsDeleted);

            var viewModel = await CreateViewModel<Casual_ViewModel>();
            viewModel.Casuals = casuals.Select(Casual_ViewModel.CasualDto.ConvertFromCasual).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutACDetails()
        {
            var casuals = await _unitOfWork.Casuals.ForListAsync(casual => string.IsNullOrWhiteSpace(casual.AdobeConnectPrincipalId));

            var viewModel = await CreateViewModel<Casual_ViewModel>();
            viewModel.Casuals = casuals.Select(Casual_ViewModel.CasualDto.ConvertFromCasual).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var casual = await _unitOfWork.Casuals.ForDetailsDisplayAsync(id);

            if (casual == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Casual_DetailsViewModel>();
            viewModel.Casual = Casual_DetailsViewModel.CasualDto.ConvertFromCasual(casual);

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        public async Task<IActionResult> Create()
        {
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = await CreateViewModel<Casual_UpdateViewModel>();
            viewModel.IsNew = true;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");

            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var casual = await _unitOfWork.Casuals.ForEditAsync(id);

            if (casual == null)
            {
                return RedirectToAction("Index");
            }

            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = await CreateViewModel<Casual_UpdateViewModel>();
            viewModel.Casual = new CasualDto
            {
                Id = casual.Id,
                FirstName = casual.FirstName,
                LastName = casual.LastName,
                PortalUsername = casual.PortalUsername,
                SchoolCode = casual.SchoolCode,
                AdobeConnectPrincipalId = casual.AdobeConnectPrincipalId,
                DateDeleted = casual.DateDeleted,
                IsDeleted = casual.IsDeleted
            };
            viewModel.IsNew = false;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name", casual.SchoolCode);

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Casual_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                var schools = await _unitOfWork.Schools.ForSelectionAsync();
                viewModel.SchoolList = new SelectList(schools, "Code", "Name", viewModel.Casual.SchoolCode);

                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                await _casualService.CreateCasual(viewModel.Casual);
            }
            else
            {
                await _casualService.UpdateCasual(viewModel.Casual.Id, viewModel.Casual);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }
    }
}