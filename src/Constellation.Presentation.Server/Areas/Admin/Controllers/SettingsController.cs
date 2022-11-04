using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Admin.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AuthRoles.Admin)]
    public class SettingsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public SettingsController(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Update()
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = await CreateViewModel<Settings_UpdateViewModel>();
            viewModel.AdobeConnectDefaultFolder = settings.AdobeConnectDefaultFolder;
            viewModel.SentralContactPreference = settings.SentralContactPreference;
            viewModel.LessonsCoordinatorEmail = settings.LessonsCoordinatorEmail;
            viewModel.LessonsCoordinatorName = settings.LessonsCoordinatorName;
            viewModel.LessonsCoordinatorTitle = settings.LessonsCoordinatorTitle;
            viewModel.LessonsHeadTeacherEmail = settings.LessonsHeadTeacherEmail;

            viewModel.SentralContactPreferenceList = new SelectList(
                new List<SelectListItem>()
                {
                    new SelectListItem {Text = "Mother first, then Father", Value = ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather},
                    new SelectListItem {Text = "Father first, then Mother", Value = ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother},
                    new SelectListItem {Text = "Both Parents", Value = ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent}
                },
                "Value",
                "Text",
                viewModel.SentralContactPreference);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Settings_UpdateViewModel viewModel)
        {
            var settings = await _unitOfWork.Settings.Get();

            settings.AdobeConnectDefaultFolder = viewModel.AdobeConnectDefaultFolder;
            settings.SentralContactPreference = viewModel.SentralContactPreference;
            settings.LessonsCoordinatorEmail = viewModel.LessonsCoordinatorEmail;
            settings.LessonsCoordinatorName = viewModel.LessonsCoordinatorName;
            settings.LessonsCoordinatorTitle = viewModel.LessonsCoordinatorTitle;
            settings.LessonsHeadTeacherEmail = viewModel.LessonsHeadTeacherEmail;

            await _unitOfWork.CompleteAsync();

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> Absences()
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = await CreateViewModel<Settings_AbsenceViewModel>();
            viewModel.AbsenceCoordinatorEmail = settings.Absences.AbsenceCoordinatorEmail;
            viewModel.AbsenceCoordinatorName = settings.Absences.AbsenceCoordinatorName;
            viewModel.AbsenceCoordinatorTitle = settings.Absences.AbsenceCoordinatorTitle;
            viewModel.ForwardingEmailAbsenceCoordinator = settings.Absences.ForwardingEmailAbsenceCoordinator;
            viewModel.ForwardingEmailTruancyOfficer = settings.Absences.ForwardingEmailTruancyOfficer;
            viewModel.DiscountedPartialReasons = settings.Absences.DiscountedPartialReasons.Expand(',').ToList();
            viewModel.DiscountedWholeReasons = settings.Absences.DiscountedWholeReasons.Expand(',').ToList();
            viewModel.AbsenceScanStartDate = settings.Absences.AbsenceScanStartDate;
            viewModel.PartialLengthThreshold = settings.Absences.PartialLengthThreshold;

            viewModel.AbsenceReasonList = new MultiSelectList(_unitOfWork.AbsenceReasons);

            return View("Absence", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Absences(Settings_AbsenceViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                viewModel.AbsenceReasonList = new MultiSelectList(_unitOfWork.AbsenceReasons);
                return View("Absence", viewModel);
            }

            var settings = await _unitOfWork.Settings.Get();

            settings.Absences.AbsenceCoordinatorEmail = viewModel.AbsenceCoordinatorEmail;
            settings.Absences.AbsenceCoordinatorName = viewModel.AbsenceCoordinatorName;
            settings.Absences.AbsenceCoordinatorTitle = viewModel.AbsenceCoordinatorTitle;
            settings.Absences.ForwardingEmailAbsenceCoordinator = viewModel.ForwardingEmailAbsenceCoordinator;
            settings.Absences.ForwardingEmailTruancyOfficer = viewModel.ForwardingEmailTruancyOfficer;
            settings.Absences.DiscountedPartialReasons = viewModel.DiscountedPartialReasons.Collapse(',');
            settings.Absences.DiscountedWholeReasons = viewModel.DiscountedWholeReasons.Collapse(',');
            settings.Absences.AbsenceScanStartDate = viewModel.AbsenceScanStartDate;
            settings.Absences.PartialLengthThreshold = viewModel.PartialLengthThreshold;

            await _unitOfWork.CompleteAsync();

            return RedirectToPage("/Index", new { area = "" });
        }
    }
}