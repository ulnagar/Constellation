using Constellation.Application.Helpers;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_StatusUpdateViewModel : BaseViewModel
    {
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public Status CurrentStatus { get; set; }
        public Status NewStatus { get; set; }
        
        [Required]
        public string Notes { get; set; }

        // View Properties
        public ICollection<SelectListItem> StatusList { get; set; }

        public void GenerateStatusList(Status currentStatus)
        {
            var excludeStatus = new[]
            {
                Status.Unknown,
                Status.New,
                Status.Allocated
            }.ToList();

            var allowedStatuses = new[]
            {
                Status.Unknown,
                Status.New,
                Status.RepairingChecking,
                Status.RepairingInternal,
                Status.RepairingReturning,
                Status.RepairingVendor
            }.ToList();

            if (!allowedStatuses.Contains(currentStatus))
            {
                excludeStatus.Add(Status.Ready);
            }

            var statuses = Enum.GetValues(typeof(Status)).Cast<int>().Where(i => i > 0);

            StatusList = new List<SelectListItem>();

            foreach (var status in statuses)
            {
                var item = new SelectListItem
                {
                    Value = status.ToString(),
                    Text = ((Status)status).GetDisplayName(),
                    Disabled = excludeStatus.Contains((Status)status) ? true : false
                };

                StatusList.Add(item);
            }
        }
    }
}