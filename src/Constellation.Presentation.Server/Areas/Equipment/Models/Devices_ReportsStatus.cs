using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_ReportsStatus : BaseViewModel
    {
        public ICollection<Devices_ReportsModel> Models { get; set; }

        public ICollection<string> StatusList { get; set; }

        public Devices_ReportsStatus()
        {
            Models = new List<Devices_ReportsModel>();
            StatusList = new List<string>();
        }
    }

    public class Devices_ReportsModel
    {
        public string Name { get; set; }
        public ICollection<Devices_ReportsDetail> Details { get; set; }

        public Devices_ReportsModel()
        {
            Details = new List<Devices_ReportsDetail>();
        }
    }

    public class Devices_ReportsDetail
    {
        public string Status { get; set; }
        public int UnallocatedValue { get; set; }
        public int AllocatedValue { get; set; }
    }
}