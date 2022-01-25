using System.ComponentModel.DataAnnotations;

namespace Constellation.Core.Enums
{
    public enum Status
    {
        [Display(Name = "Unknown")]
        Unknown = 0,
        [Display(Name = "New")]
        New = 1,
        [Display(Name = "In Use")]
        Allocated = 2,
        [Display(Name = "Ready")]
        Ready = 3,
        [Display(Name = "Repairing: Returning")]
        RepairingReturning = 4,
        [Display(Name = "Repairing: Checking")]
        RepairingChecking = 5,
        [Display(Name = "Repairing: Internal")]
        RepairingInternal = 6,
        [Display(Name = "Repairing: Vendor")]
        RepairingVendor = 7,
        [Display(Name = "On Hold")]
        OnHold = 8,
        [Display(Name = "Written Off: Withdrawn")]
        WrittenOffWithdrawn = 9,
        [Display(Name = "Written Off: Replaced")]
        WrittenOffReplaced = 10,
        [Display(Name = "Written Off: Damaged")]
        WrittenOffDamaged = 11,
        [Display(Name = "Written Off: Lost")]
        WrittenOffLost = 12
    }
}