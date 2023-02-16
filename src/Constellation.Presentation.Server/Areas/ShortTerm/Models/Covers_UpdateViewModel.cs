using Constellation.Application.Helpers;
using Constellation.Core.Models.Covers;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Covers_UpdateViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string CoverType { get; set; }
        public string UserName { get; set; }
        [Display(Name = DisplayNameDefaults.ClassName)]
        public string OfferingName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name=DisplayNameDefaults.DateStart)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name=DisplayNameDefaults.DateEnd)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public static void GetFromCasualCover(CasualClassCover cover, Covers_UpdateViewModel viewModel)
        {
            viewModel.Id = cover.Id;
            viewModel.CoverType = "Casual";
            viewModel.UserName = cover.Casual.DisplayName;
            viewModel.OfferingName = cover.Offering.Name;
            viewModel.StartDate = cover.StartDate;
            viewModel.EndDate = cover.EndDate;
        }

        public static void GetFromTeacherCover(TeacherClassCover cover, Covers_UpdateViewModel viewModel)
        {
            viewModel.Id = cover.Id;
            viewModel.CoverType = "Teacher";
            viewModel.UserName = cover.Staff.DisplayName;
            viewModel.OfferingName = cover.Offering.Name;
            viewModel.StartDate = cover.StartDate;
            viewModel.EndDate = cover.EndDate;
        }
    }
}