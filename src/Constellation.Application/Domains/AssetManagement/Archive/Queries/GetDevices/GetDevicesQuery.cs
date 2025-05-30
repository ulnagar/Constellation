﻿namespace Constellation.Application.Domains.AssetManagement.Archive.Queries.GetDevices;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetDevicesQuery()
    : IQuery<List<DeviceSummaryResponse>>;