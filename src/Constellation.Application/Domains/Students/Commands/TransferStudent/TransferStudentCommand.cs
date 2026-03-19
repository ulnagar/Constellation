namespace Constellation.Application.Domains.Students.Commands.TransferStudent;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System;

public sealed record TransferStudentCommand(
    StudentId StudentId,
    SchoolCode SchoolCode,
    Grade Grade,
    DateOnly StartDate)
    : ICommand;