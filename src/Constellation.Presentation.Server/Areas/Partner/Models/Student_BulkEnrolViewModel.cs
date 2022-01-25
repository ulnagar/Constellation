using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_BulkEnrolViewModel : BaseViewModel
    {
        public string StudentId { get; set; }
        public ICollection<int> SelectedClasses { get; set; }

        public ICollection<CourseOffering> OfferingList { get; set; }
    }
}