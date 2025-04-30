namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetDevicesAllocatedToStudent;

using Constellation.Core.Models.Assets.ValueObjects;
using System;

public sealed record StudentDeviceResponse(
    AssetNumber AssetNumber,
    string SerialNumber,
    string Make,
    string Status,
    DateOnly DateAllocated,
    DateOnly DateReturned);