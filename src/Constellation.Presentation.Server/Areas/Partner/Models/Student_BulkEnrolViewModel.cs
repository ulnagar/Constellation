using Constellation.Application.Offerings.GetOfferingsForBulkEnrol;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_BulkEnrolViewModel : BaseViewModel
    {
        public string StudentId { get; set; }
        public ICollection<Guid> SelectedClasses { get; set; }

        public List<BulkEnrolOfferingResponse> OfferingList { get; set; } = new();
    }
}