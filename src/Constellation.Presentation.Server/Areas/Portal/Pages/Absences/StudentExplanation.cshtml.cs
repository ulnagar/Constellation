using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences
{
    public class StudentExplanationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAbsenceService _absenceService;

        public StudentExplanationModel(IUnitOfWork unitOfWork, IAbsenceService absenceService)
        {
            _unitOfWork = unitOfWork;
            _absenceService = absenceService;
        }

        [BindProperty]
        public AbsenceDto Absence { get; set; }

        public async Task<IActionResult> OnGet(Guid id)
        {
            var absence = await _unitOfWork.Absences.ForExplanationFromParent(id);

            if (absence == null)
                throw new ArgumentOutOfRangeException(nameof(id));

            Absence = new AbsenceDto
            {
                AbsenceId = absence.Id,
                StudentName = absence.Student.DisplayName,
                StudentId = absence.Student.StudentId,
                Date = absence.Date,
                Type = absence.Type,
                OfferingId = absence.OfferingId,
                OfferingName = absence.Offering.Name,
                PeriodName = absence.PeriodName,
                PeriodTimeframe = absence.PeriodTimeframe,
                IsExplained = absence.Explained,
                AbsenceLength = absence.AbsenceLength,
                AbsenceTimeframe = absence.AbsenceTimeframe
            };

            var response = absence.Responses.FirstOrDefault(re => re.VerificationStatus == AbsenceResponse.Verified);

            if (response != null)
            {
                Absence.Reason = response.Explanation;
                Absence.VerifiedBy = response.Verifier;
                Absence.IsVerified = true;
            } 
            else if (absence.ExternallyExplained)
            {
                Absence.Reason = absence.ExternalExplanation;
                Absence.VerifiedBy = absence.ExternalExplanationSource;
                Absence.IsVerified = true;
            }
            else
            {
                Absence.IsVerified = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (Absence.Reason == null)
                ModelState.AddModelError("Reason", "You must provide an explanation for this absence.");

            if (!ModelState.IsValid)
                return Page();

            await _absenceService.CreateSingleStudentExplanation(Absence.AbsenceId, Absence.Reason);

            return RedirectToPage("/Absences/Students", new { area = "Portal", studentId = Absence.StudentId });
        }

        public class AbsenceDto
        {
            [Required]
            public Guid AbsenceId { get; set; }
            [Display(Name =DisplayNameDefaults.DisplayName)]
            public string StudentName { get; set; }
            [Required]
            public string StudentId { get; set; }
            public DateTime Date { get; set; }
            public string Type { get; set; }
            public int OfferingId { get; set; }
            [Display(Name =DisplayNameDefaults.ClassName)]
            public string OfferingName { get; set; }
            [Display(Name = DisplayNameDefaults.PeriodName)]
            public string PeriodName { get; set; }
            [Display(Name = DisplayNameDefaults.PeriodTimeframe)]
            public string PeriodTimeframe { get; set; }
            [Required]
            public string Reason { get; set; }
            public bool IsExplained { get; set; }
            public bool IsVerified { get; set; }
            public string VerifiedBy { get; set; }
            [Display(Name = DisplayNameDefaults.AbsenceTimeframe)]
            public string AbsenceTimeframe { get; set; }
            public int AbsenceLength { get; set; }
        }
    }
}
