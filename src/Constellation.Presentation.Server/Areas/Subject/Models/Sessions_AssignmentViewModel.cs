using Constellation.Core.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Sessions_AssignmentViewModel : BaseViewModel
    {
        public OfferingId OfferingId { get; set; }
        public int PeriodId { get; set; }
        public string TeacherId { get; set; }
        public string RoomId { get; set; }
        public string OfferingName { get; set; }
        public SelectList PeriodList { get; set; }
        public SelectList StaffList { get; set; }
        public SelectList RoomList { get; set; }
    }
}