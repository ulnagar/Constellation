namespace Constellation.Application.Devices.GetDeviceDetails;

using Abstractions.Messaging;

public sealed record GetDeviceDetailsQuery(
    string SerialNumber)
    : IQuery<DeviceDetailsResponse>;