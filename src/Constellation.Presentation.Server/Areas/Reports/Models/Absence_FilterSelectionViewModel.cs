using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Absence_FilterSelectionViewModel : BaseViewModel
    {
        public AbsenceFilterDto Filter { get; set; }
        public SelectList StudentList { get; set; }
        public SelectList SchoolList { get; set; }
    }
}