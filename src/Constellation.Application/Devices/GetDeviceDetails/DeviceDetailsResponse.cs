namespace Constellation.Application.Devices.GetDeviceDetails;

using Core.Enums;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record DeviceDetailsResponse(
    string Make,
    string Model,
    string SerialNumber,
    string Description,
    Status Status,
    DateTime? DateReceived,
    DateTime? DateWarrantyExpires,
    DateTime? DateDisposed,
    List<DeviceDetailsResponse.Allocation> Allocations,
    List<DeviceDetailsResponse.Note> Notes)
{
    public sealed record Note(
        DateTime DateEntered,
        string Details);

    public sealed record Allocation(
        DateTime DateAllocated,
        Name Student,
        Grade Grade,
        string School,
        DateTime? DateDeleted);
}