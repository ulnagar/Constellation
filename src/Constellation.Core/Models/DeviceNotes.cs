namespace Constellation.Core.Models;

using System;

public class DeviceNotes
{
    public int Id { get; set; }
    public string SerialNumber { get; set; }
    public Device Device { get; set; }
    public DateTime DateEntered { get; set; }
    public string Details { get; set; }
}