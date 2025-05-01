namespace Constellation.Application.Domains.AssetManagement.Archive.Queries.GetDeviceDetails;

using Abstractions.Messaging;

public sealed record GetDeviceDetailsQuery(
    string SerialNumber)
    : IQuery<DeviceDetailsResponse>;