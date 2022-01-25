using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class School_UpdateViewModel : BaseViewModel
    {
        // School object
        public SchoolDto Resource { get; set; }

        // View Properties
        public int id { get; set; }
        public bool IsNew { get; set; }
    }
}