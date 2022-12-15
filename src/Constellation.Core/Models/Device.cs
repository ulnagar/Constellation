namespace Constellation.Core.Models;

using Constellation.Core.Enums;

public class Device
{
    public Device()
    {
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

    public string SerialNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime? DateWarrantyExpires { get; set; }
    public DateTime? DateReceived { get; set; }
    public DateTime? DateDisposed { get; set; }
    public List<DeviceAllocation> Allocations { get; set; } = new();
    public List<DeviceNotes> Notes { get; set; } = new();

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

    //TODO: Move this to the Enumeration level? Or incorporate in the update method of the model
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