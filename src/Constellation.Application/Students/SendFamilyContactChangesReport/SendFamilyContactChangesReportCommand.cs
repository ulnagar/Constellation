﻿namespace Constellation.Application.Students.SendFamilyContactChangesReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Infrastructure.Jobs;
using System.Collections.Generic;

public sealed record SendFamilyContactChangesReportCommand(
    List<ParentContactChangeDto> changes)
    : ICommand;