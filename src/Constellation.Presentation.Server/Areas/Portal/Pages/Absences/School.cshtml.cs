using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Absences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences
{
    [Authorize]
    public class SchoolModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchoolModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            Schools = new Dictionary<string, string>();
        }

        [BindProperty(SupportsGet = true)]
        public string SchoolCode { get; set; }
        [BindProperty(SupportsGet = true)]
        public DataSet SelectedDataSet { get; set; } = DataSet.UnverifiedPartial;
        public IDictionary<string, string> Schools { get; set; }
        public SelectedData Data { get; set; }

        public async Task<IActionResult> OnGet()
        {
            // 1. Get the current user
            // 2. Get the list of school the user is attached to
            // 2.1 IF SchoolCode has a value, check that the user is authorised for that school
            // 3. Get the absence data per school
            // 4. Show the absence data for the first school (or the school indicated by SchoolCode if applicable)

            var username = User.Identity.Name;
            var coordinator = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(username);

            if (coordinator == null)
                return RedirectToPage("/Error");
            
            var schools = coordinator.Assignments.Where(assign => !assign.IsDeleted).OrderBy(assign => assign.School.Name).Select(assign => assign.SchoolCode).Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(SchoolCode) && !schools.Contains(SchoolCode))
                SchoolCode = string.Empty;

            foreach (var school in schools)
            {
                Schools.Add(school, coordinator.Assignments.First(assign => assign.SchoolCode == school).School.Name);
            }

            var selectedSchoolCode = (string.IsNullOrWhiteSpace(SchoolCode)) ? Schools.FirstOrDefault().Key : Schools.FirstOrDefault(school => school.Key == SchoolCode).Key;

            if (selectedSchoolCode == null)
                return Page();

            Data = new SelectedData
            {
                Code = selectedSchoolCode,
                Name = Schools.First(school => school.Key == selectedSchoolCode).Value
            };

            var schoolData = await _unitOfWork.Absences.AllFromSchoolForCoordinatorPortal(Data.Code);

            schoolData = schoolData.Where(absence => absence.Date.Year == DateTime.Now.Year).ToList();

            bool unexplainedPartialsPredicate(Absence absence) => !absence.Responses.Any() && !absence.Explained && absence.Type == Absence.Partial;
            bool unverifiedPartialsPredicate(Absence absence) => absence.Type == Absence.Partial && absence.Responses.Any(response => response.VerificationStatus == AbsenceResponse.Pending) && !absence.Explained;
            bool unexplainedWholesPredicate(Absence absence) => !absence.Responses.Any() && !absence.Explained && absence.Type == Absence.Whole;

            Data.UnexplainedPartials = schoolData.Count(unexplainedPartialsPredicate);
            Data.UnverifiedPartials = schoolData.Count(unverifiedPartialsPredicate);
            Data.UnexplainedWholes = schoolData.Count(unexplainedWholesPredicate);

            switch (SelectedDataSet)
            {
                case DataSet.UnexplainedPartial:
                    Data.Absences = schoolData.Where(unexplainedPartialsPredicate)
                        .Select(AbsenceDto.ConvertFromAbsence)
                        .ToList();
                    break;
                case DataSet.UnexplainedWhole:
                    Data.Absences = schoolData.Where(unexplainedWholesPredicate)
                        .Select(AbsenceDto.ConvertFromAbsence)
                        .ToList();
                    break;
                case DataSet.UnverifiedPartial:
                    Data.Absences = schoolData.Where(unverifiedPartialsPredicate)
                        .Select(AbsenceDto.ConvertFromAbsence)
                        .ToList();
                    break;
            }

            return Page();
        }

        public class SelectedData
        {
            public SelectedData()
            {
                Absences = new List<AbsenceDto>();
            }

            public string Code { get; set; }
            public string Name { get; set; }
            public int UnexplainedPartials { get; set; }
            public int UnverifiedPartials { get; set; }
            public int UnexplainedWholes { get; set; }
            public string DataSetName { get; set; }
            public ICollection<AbsenceDto> Absences { get; set; }
        }

        public class AbsenceDto
        {
            public Guid AbsenceId { get; set; }
            public Guid? AbsenceReasonId { get; set; }
            public string StudentName { get; set; }
            public DateTime AbsenceDate { get; set; }
            public string ClassName { get; set; }
            public string PeriodName { get; set; }
            public string PeriodTimeframe { get; set; }
            public string AbsenceTimeframe { get; set; }
            public int AbsenceLength { get; set; }
            public string Explanation { get; set; }

            public static AbsenceDto ConvertFromAbsence(Absence absence)
            {
                var viewModel = new AbsenceDto
                {
                    AbsenceId = absence.Id,
                    AbsenceReasonId = absence.Responses.FirstOrDefault(response => response.VerificationStatus == AbsenceResponse.Pending)?.Id,
                    StudentName = absence.Student.DisplayName,
                    AbsenceDate = absence.Date,
                    ClassName = absence.Offering.Name,
                    PeriodName = absence.PeriodName,
                    PeriodTimeframe = absence.PeriodTimeframe,
                    AbsenceLength = absence.AbsenceLength,
                    AbsenceTimeframe = absence.AbsenceTimeframe,
                    Explanation = absence.Responses.FirstOrDefault(response => response.VerificationStatus == AbsenceResponse.Pending)?.Explanation
                };

                return viewModel;
            }
        }

        public enum DataSet
        {
            UnexplainedPartial,
            UnexplainedWhole,
            UnverifiedPartial
        }
    }
}
