using Constellation.Application.Features.Equipment.Stocktake.Models;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Stocktake.RegisterSighting;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake
{
    public class StaffStocktakeSightingViewModel : BaseViewModel
    {
        public Guid StocktakeEventId { get; set; }
        public string SerialNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Description { get; set; }
        public string LocationCategory { get; set; }
        public string LocationName { get; set; }
        public string LocationCode { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string Comment { get; set; }

        public List<StudentSelectionResponse> Students { get; set; } = new();
        public List<StaffSelectionListResponse> Staff { get; set; } = new();
        public List<PartnerSchoolForDropdownSelection> Schools { get; set; } = new();
    }
}
