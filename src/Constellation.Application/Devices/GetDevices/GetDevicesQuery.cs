namespace Constellation.Application.Devices.GetDevices;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetDevicesQuery()
    : IQuery<List<DeviceSummaryResponse>>;