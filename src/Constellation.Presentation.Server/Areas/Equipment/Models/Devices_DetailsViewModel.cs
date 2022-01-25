using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Equipment.Models
{
    public class Devices_DetailsViewModel : BaseViewModel
    {
        public DeviceDto Device { get; set; }
        public ICollection<AllocationDto> Allocations { get; set; }
        public ICollection<NoteDto> Notes { get; set; }

        public class DeviceDto
        {
            public string Make { get; set; }
            public string Model { get; set; }
            public string SerialNumber { get; set; }
            public string Description { get; set; }
            public Status Status { get; set; }
            public DateTime? DateReceived { get; set; }
            public DateTime? DateWarrantyExpires { get; set; }
            public DateTime? DateDisposed { get; set; }
            public bool CanAllocate { get; set; }
            public bool CanUpdateStatus { get; set; }
            public bool IsAllocated { get; set; }

            public static DeviceDto ConvertFromDevice(Device device)
            {
                var viewModel = new DeviceDto
                {
                    Make = device.Make,
                    Model = device.Model,
                    SerialNumber = device.SerialNumber,
                    Description = device.Description,
                    Status = device.Status,
                    DateReceived = device.DateReceived,
                    DateDisposed = device.DateDisposed,
                    DateWarrantyExpires = device.DateWarrantyExpires,
                    CanAllocate = device.CanAllocate(),
                    CanUpdateStatus = device.CanUpdateStatus(),
                    IsAllocated = device.IsAllocated()
                };

                return viewModel;
            }
        }

        public class AllocationDto
        {
            public DateTime DateAllocated { get; set; }
            public string StudentName { get; set; }
            public Grade StudentGrade { get; set; }
            public string StudentSchool { get; set; }
            public DateTime? DateDeleted { get; set; }

            public static AllocationDto ConvertFromAllocation(DeviceAllocation allocation)
            {
                var viewModel = new AllocationDto
                {
                    DateAllocated = allocation.DateAllocated,
                    DateDeleted = allocation.DateDeleted,
                    StudentName = allocation.Student.DisplayName,
                    StudentGrade = allocation.Student.CurrentGrade,
                    StudentSchool = allocation.Student.School.Name
                };

                return viewModel;
            }
        }

        public class NoteDto
        {
            public DateTime DateEntered { get; set; }
            public string Details { get; set; }

            public static NoteDto ConvertFromNote(DeviceNotes note)
            {
                var viewModel = new NoteDto
                {
                    DateEntered = note.DateEntered,
                    Details = note.Details
                };

                return viewModel;
            }
        }
    }
}