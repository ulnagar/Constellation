namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using Core.Models.Assets.ValueObjects;
using System;

public sealed record StudentDeviceResponse(
    AssetNumber AssetNumber,
    string SerialNumber,
    string Make,
    string Status,
    DateOnly DateAllocated,
    DateOnly DateReturned);