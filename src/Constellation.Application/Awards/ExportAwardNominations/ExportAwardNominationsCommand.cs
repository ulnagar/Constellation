namespace Constellation.Application.Awards.ExportAwardNominations;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Identifiers;

public sealed record ExportAwardNominationsCommand(
    AwardNominationPeriodId PeriodId)
    : ICommand<FileDto>;