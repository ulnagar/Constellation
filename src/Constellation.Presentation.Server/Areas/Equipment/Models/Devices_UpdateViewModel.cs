using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_UpdateViewModel : BaseViewModel
    {
        // Device object
        public DeviceResource Device { get; set; }

        // View Properties
        public bool IsNew { get; set; }

        public Devices_UpdateViewModel()
        {
            Device = new DeviceResource();
        }
    }
}