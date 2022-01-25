using Constellation.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class DeviceResource
    {
        [Required]
        public string SerialNumber { get; set; }

        public string Make { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? DateWarrantyExpires { get; set; }

        public Status Status { get; set; } = Status.New;
    }
}