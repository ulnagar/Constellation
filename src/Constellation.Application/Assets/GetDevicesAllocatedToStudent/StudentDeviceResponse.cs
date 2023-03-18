namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using System;

public sealed record StudentDeviceResponse(
    string SerialNumber,
    string Make,
    string Status,
    DateOnly DateAllocated,
    DateOnly? DateReturned);