namespace Constellation.Application.Domains.SciencePracs.Commands.BulkCancelRolls;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record BulkCancelRollsCommand(
    List<SchoolCode> SchoolCodes,
    List<Grade> Grades,
    string Comment)
    : ICommand;
