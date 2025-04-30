namespace Constellation.Application.Models;

using System;

public class JobActivation
{
    public Guid Id { get; set; }
    public string JobName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? InactiveUntil { get; set; }
}