using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_AssignViewModel : BaseViewModel
    {
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public string StudentId { get; set; }

        // View Properties
        public SelectList StudentList { get; set; }
    }
}