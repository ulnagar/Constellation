using Constellation.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences
{
    public class ParentsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ParentsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            Explanations = new List<AbsenceDto>();
        }

        [BindProperty(SupportsGet = true)]
        public bool ShowAll { get; set; }

        [BindProperty]
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        public ICollection<AbsenceDto> Explanations { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        public async Task<IActionResult> OnGet(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentOutOfRangeException(nameof(studentId));
            }

            // Get all absences for this student from this year.
            var absences = await _unitOfWork.Absences.AllFromStudentForParentPortal(studentId);
            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            StudentId = student.StudentId;
            StudentName = student.DisplayName;
            
            // If there are any absences that do not have a response, continue
            if (absences.Count > 0)
            {
                foreach (var absence in absences.OrderBy(a => a.Date).ThenBy(a => a.PeriodTimeframe))
                {
                    Explanations.Add(
                        new AbsenceDto()
                        {
                            AbsenceId = absence.Id,
                            Date = absence.Date.Date,
                            Type = absence.Type,
                            OfferingId = absence.OfferingId,
                            OfferingName = absence.Offering.Name,
                            PeriodName = absence.PeriodName,
                            PeriodTimeframe = absence.PeriodTimeframe,
                            IsExplained = absence.Explained
                        });
                }
            }

            if (!ShowAll)
                Explanations = Explanations.Where(entry => !entry.IsExplained).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostReport()
        {
            return RedirectToAction("AttendanceReportDownload", "Absences", new { area = "Reports", studentId = StudentId, startDate = ReportDate });
        }

        public class AbsenceDto
        {
            public Guid AbsenceId { get; set; }
            public DateTime Date { get; set; }
            public string Type { get; set; }
            public int OfferingId { get; set; }
            public string OfferingName { get; set; }
            public string PeriodName { get; set; }
            public string PeriodTimeframe { get; set; }
            public string Reason { get; set; }
            public bool IsExplained { get; set; }
        }
    }
}
