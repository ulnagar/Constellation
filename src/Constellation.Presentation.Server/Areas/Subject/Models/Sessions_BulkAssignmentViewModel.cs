using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Sessions_BulkAssignmentViewModel : BaseViewModel
    {
        public Sessions_BulkAssignmentViewModel()
        {
            Periods = new List<int>();
            ValidPeriods = new List<TimetablePeriod>();
        }

        public int OfferingId { get; set; }
        public IEnumerable<int> Periods { get; set; }
        public string TeacherId { get; set; }
        public string RoomId { get; set; }
        public string OfferingName { get; set; }
        public IEnumerable<TimetablePeriod> ValidPeriods { get; set; }
        public SelectList StaffList { get; set; }
        public SelectList RoomList { get; set; }
    }
}