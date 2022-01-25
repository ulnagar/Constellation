using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_ViewModel : BaseViewModel
    {
        public Devices_ViewModel()
        {
            Devices = new List<DeviceDto>();
            ListOfMakes = new List<string>();
        }

        public ICollection<DeviceDto> Devices { get; set; }
        public ICollection<string> ListOfMakes { get; set; }

        public class DeviceDto
        {
            public string SerialNumber { get; set; }
            public string Make { get; set; }
            public Status Status { get; set; }
            public string StudentName { get; set; }
            public Grade StudentGrade { get; set; }
            public string StudentSchool { get; set; }

            public static DeviceDto ConvertFromDevice(Device device)
            {
                var viewModel = new DeviceDto
                {
                    SerialNumber = device.SerialNumber,
                    Make = device.Make,
                    Status = device.Status
                };

                var allocation = device.Allocations.FirstOrDefault(loan => !loan.IsDeleted);

                if (allocation != null)
                {
                    viewModel.StudentName = allocation.Student.DisplayName;
                    viewModel.StudentGrade = allocation.Student.CurrentGrade;
                    viewModel.StudentSchool = allocation.Student.School.Name;
                }

                return viewModel;
            }
        }
    }
}