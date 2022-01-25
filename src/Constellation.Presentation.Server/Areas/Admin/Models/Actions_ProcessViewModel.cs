using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Actions_ProcessViewModel : BaseViewModel
    {
        public bool Success { get; set; }
        public ICollection<string> Statuses { get; set; }

        public Actions_ProcessViewModel()
        {
            Statuses = new List<string>();
        }
    }
}