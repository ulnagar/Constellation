namespace Constellation.Application.SciencePracs.BulkCancelRolls;

using Abstractions.Messaging;
using Core.Enums;
using System.Collections.Generic;

public sealed record BulkCancelRollsCommand(
    List<string> SchoolCodes,
    List<Grade> Grades,
    string Comment)
    : ICommand;
