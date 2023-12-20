﻿namespace Constellation.Application.Training.Modules.GenerateOverallReport;

using Abstractions.Messaging;
using DTOs;

public sealed record GenerateOveralReportCommand
    : ICommand<FileDto>;