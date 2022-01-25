using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_CreateNoteViewModel : BaseViewModel
    {
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public string Notes { get; set; }
    }
}