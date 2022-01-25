using Constellation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Core.Models
{
    public class Device
    {
        public Device()
        {
            Notes = new List<DeviceNotes>();
            Allocations = new List<DeviceAllocation>();

            DateReceived = DateTime.Now;
            Status = Status.New;
        }

        public Device(string serialNumber, Status status)
        {
            Notes = new List<DeviceNotes>();
            Allocations = new List<DeviceAllocation>();

            DateReceived = DateTime.Now;

            SerialNumber = serialNumber;
            Status = status;
        }

        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public DateTime? DateWarrantyExpires { get; set; }
        public DateTime? DateReceived { get; set; }
        public DateTime? DateDisposed { get; set; }
        public ICollection<DeviceAllocation> Allocations { get; set; }
        public ICollection<DeviceNotes> Notes { get; set; }

        public bool CanAllocate()
        {
            if (IsAllocated())
                return false;

            if (Status == Status.New || Status == Status.Ready)
                return true;

            return false;
        }

        public bool IsAllocated()
        {
            if (Allocations.Any(a => !a.IsDeleted))
                return true;

            return false;
        }

        public bool CanUpdateStatus()
        {
            switch (Status)
            {
                case Status.Unknown:
                case Status.New:
                case Status.Allocated:
                case Status.Ready:
                case Status.RepairingReturning:
                case Status.RepairingChecking:
                case Status.RepairingInternal:
                case Status.RepairingVendor:
                case Status.OnHold:
                    return true;
                case Status.WrittenOffWithdrawn:
                case Status.WrittenOffReplaced:
                case Status.WrittenOffDamaged:
                case Status.WrittenOffLost:
                default:
                    return false;
            }
        }
    }
}