﻿namespace Constellation.Application.Students.TransferStudent;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Students.Identifiers;
using System;

public sealed record TransferStudentCommand(
    StudentId StudentId,
    string SchoolCode,
    Grade Grade,
    DateOnly StartDate)
    : ICommand;