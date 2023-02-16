using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences
{
    [Authorize]
    public class SchoolVerificationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAbsenceService _absenceService;

        public SchoolVerificationModel(IUnitOfWork unitOfWork, IAbsenceService absenceService)
        {
            _unitOfWork = unitOfWork;
            _absenceService = absenceService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public AbsenceDto DisplayAbsence { get; set; }
        [BindProperty]
        public bool IsVerified { get; set; }
        [BindProperty]
        public string Comment { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var username = User.Identity.Name;
            var coordinator = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(username);

            if (coordinator == null)
                return NotFound();

            if (Id == Guid.Empty)
                return RedirectToPage("School", new { area = "Portal" });

            var response = await _unitOfWork.Absences.AsResponseForVerificationByCoordinator(Id);
            if (response == null || response.Absence.Type != Absence.Partial)
                return RedirectToPage("School", new { area = "Portal" });

            DisplayAbsence = AbsenceDto.ConvertFromAbsenceResponse(response);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!IsVerified && string.IsNullOrWhiteSpace(Comment))
                ModelState.AddModelError("Comment", "You must enter a comment if the explanation is being rejected!");

            if (!ModelState.IsValid)
            {
                var response = await _unitOfWork.Absences.AsResponseForVerificationByCoordinator(Id);
                DisplayAbsence = AbsenceDto.ConvertFromAbsenceResponse(response);

                return Page();
            }

            await _absenceService.RecordCoordinatorVerificationOfPartialExplanation(Id, IsVerified, Comment, User.Identity.Name);

            return RedirectToPage("School", new { area = "Portal" });
        }

        public class AbsenceDto
        {
            [Display(Name = DisplayNameDefaults.Student)]
            public string StudentName { get; set; }
            [Display(Name = DisplayNameDefaults.ClassName)]
            public string ClassName { get; set; }
            public DateTime Date { get; set; }
            [Display(Name = DisplayNameDefaults.PeriodName)]
            public string PeriodName { get; set; }
            [Display(Name = DisplayNameDefaults.PeriodTimeframe)]
            public string PeriodTimeframe { get; set; }
            [Display(Name = DisplayNameDefaults.AbsenceTimeframe)]
            public string AbsenceTimeframe { get; set; }
            [Display(Name = DisplayNameDefaults.AbsenceLength)]
            public int AbsenceLength { get; set; }
            public string Explanation { get; set; }

            public static AbsenceDto ConvertFromAbsenceResponse(AbsenceResponse response)
            {
                var viewModel = new AbsenceDto
                {
                    StudentName = response.Absence.Student.DisplayName,
                    ClassName = response.Absence.Offering.Name,
                    Date = response.Absence.Date,
                    PeriodName = response.Absence.PeriodName,
                    PeriodTimeframe = response.Absence.PeriodTimeframe,
                    AbsenceTimeframe = response.Absence.AbsenceTimeframe,
                    AbsenceLength = response.Absence.AbsenceLength,
                    Explanation = response.Explanation
                };

                return viewModel;
            }
        }
    }
}
