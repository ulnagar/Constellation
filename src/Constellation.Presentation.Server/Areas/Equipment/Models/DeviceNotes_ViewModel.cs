using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class DeviceNotes_ViewModel : BaseViewModel
    {
        public ICollection<DeviceNotes> Notes { get; set; }
    }
}