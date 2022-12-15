namespace Constellation.Core.Models;

public class DeviceNotes
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public virtual Device? Device { get; set; }
    public DateTime DateEntered { get; set; }
    public string Details { get; set; } = string.Empty;
}