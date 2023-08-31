using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_BulkEnrolViewModel : BaseViewModel
    {
        public string StudentId { get; set; }
        public ICollection<Guid> SelectedClasses { get; set; }

        public ICollection<Offering> OfferingList { get; set; }
    }
}