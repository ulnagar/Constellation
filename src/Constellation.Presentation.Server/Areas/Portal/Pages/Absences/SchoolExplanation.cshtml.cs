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
    public class SchoolExplanationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAbsenceService _absenceService;

        public SchoolExplanationModel(IUnitOfWork unitOfWork, IAbsenceService absenceService)
        {
            _unitOfWork = unitOfWork;
            _absenceService = absenceService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public AbsenceDto DisplayAbsence { get; set; }
        [BindProperty]
        [Required]
        public string Explanation { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var username = User.Identity.Name;
            var coordinator = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(username);

            if (coordinator == null)
                return NotFound();

            if (Id == Guid.Empty)
                return RedirectToPage("School", new { area = "Portal" });

            var absence = await _unitOfWork.Absences.ForExplanationFromParent(Id);
            if (absence == null || absence.Type == Absence.Partial)
                return RedirectToPage("School", new { area = "Portal" });

            DisplayAbsence = AbsenceDto.ConvertFromAbsence(absence);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                var absence = await _unitOfWork.Absences.ForExplanationFromParent(Id);
                DisplayAbsence = AbsenceDto.ConvertFromAbsence(absence);

                return Page();
            }

            await _absenceService.CreateSingleCoordinatorExplanation(Id, Explanation, User.Identity.Name);

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

            public static AbsenceDto ConvertFromAbsence(Absence absence)
            {
                var viewModel = new AbsenceDto
                {
                    StudentName = absence.Student.DisplayName,
                    ClassName = absence.Offering.Name,
                    Date = absence.Date,
                    PeriodName = absence.PeriodName,
                    PeriodTimeframe = absence.PeriodTimeframe
                };

                return viewModel;
            }
        }
    }
}
