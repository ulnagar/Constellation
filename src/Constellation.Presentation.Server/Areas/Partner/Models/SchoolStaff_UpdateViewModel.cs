using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class SchoolStaff_UpdateViewModel : BaseViewModel
    {
        public SchoolContactDto Contact { get; set; }
        public bool IsNew { get; set; }
    }
}