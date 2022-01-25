using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Settings_AbsenceViewModel : BaseViewModel
    {
        public IList<string> DiscountedWholeReasons { get; set; }
        public IList<string> DiscountedPartialReasons { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AbsenceScanStartDate { get; set; }

        public int PartialLengthThreshold { get; set; }

        [EmailAddress]
        public string ForwardingEmailAbsenceCoordinator { get; set; }

        [EmailAddress]
        public string ForwardingEmailTruancyOfficer { get; set; }

        public string AbsenceCoordinatorName { get; set; }

        public string AbsenceCoordinatorTitle { get; set; }

        [EmailAddress]
        public string AbsenceCoordinatorEmail { get; set; }


        public MultiSelectList AbsenceReasonList { get; set; }

        public Settings_AbsenceViewModel()
        {
            DiscountedWholeReasons = new List<string>();
            DiscountedPartialReasons = new List<string>();

            AbsenceScanStartDate = DateTime.Today;
        }
    }
}