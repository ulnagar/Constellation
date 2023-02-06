using Constellation.Core.Models.Covers;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Covers_DetailsViewModel : BaseViewModel
    {
        public CoverDto Cover { get; set; }

        public class CoverDto
        {
            public int Id { get; set; }
            [Display(Name=DisplayNameDefaults.DisplayName)]
            public string UserName { get; set; }
            [Display(Name = DisplayNameDefaults.SchoolName)]
            public string UserSchool { get; set; }
            [Display(Name = DisplayNameDefaults.ClassName)]
            public string OfferingName { get; set; }
            [Display(Name = DisplayNameDefaults.DateStart)]
            public DateTime StartDate { get; set; }
            [Display(Name = DisplayNameDefaults.DateEnd)]
            public DateTime EndDate { get; set; }

            public static CoverDto ConvertFromCover<T>(T cover) where T : ClassCover
            {
                var viewModel = new CoverDto
                {
                    Id = cover.Id,
                    EndDate = cover.EndDate,
                    StartDate = cover.StartDate,
                    OfferingName = cover.Offering.Name
                };

                switch (cover)
                {
                    case CasualClassCover casual:
                        viewModel.UserName = casual.Casual.DisplayName;
                        viewModel.UserSchool = casual.Casual.School.Name;
                        break;
                    case TeacherClassCover teacher:
                        viewModel.UserName = teacher.Staff.DisplayName;
                        viewModel.UserSchool = teacher.Staff.School.Name;
                        break;
                }

                return viewModel;
            }
        }
    }
}