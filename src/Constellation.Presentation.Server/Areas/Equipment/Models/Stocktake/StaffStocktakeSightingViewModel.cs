using Constellation.Application.Features.Equipment.Stocktake.Models;
using Constellation.Application.Features.Portal.School.Home.Models;
using Constellation.Application.Features.Portal.School.Stocktake.Commands;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake
{
    public class StaffStocktakeSightingViewModel : BaseViewModel
    {
        public RegisterSightedDeviceForStocktakeCommand Command { get; set; } = new();

        public List<StudentFromSchoolForDropdownSelection> Students { get; set; } = new();
        public List<StaffFromSchoolForDropdownSelection> Staff { get; set; } = new();
        public List<PartnerSchoolForDropdownSelection> Schools { get; set; } = new();
    }
}
