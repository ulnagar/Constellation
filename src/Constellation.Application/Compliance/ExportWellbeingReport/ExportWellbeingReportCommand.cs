namespace Constellation.Application.Compliance.ExportWellbeingReport;

using Abstractions.Messaging;
using DTOs;
using GetWellbeingReportFromSentral;
using System.Collections.Generic;

public sealed record ExportWellbeingReportCommand(
    List<SentralIncidentDetails> Records)
    : ICommand<FileDto>;