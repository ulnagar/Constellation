﻿namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateCustomReportForPeriod;

using Constellation.Application.Abstractions.Messaging;
using System.IO;

public sealed record GenerateCustomReportForPeriodQuery(
    string PeriodLabel,
    string ValueType)
    : IQuery<MemoryStream>;