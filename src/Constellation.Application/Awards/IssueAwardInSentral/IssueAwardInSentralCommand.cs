﻿namespace Constellation.Application.Awards.IssueAwardInSentral;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Enums;
using System.Collections.Generic;

public sealed record IssueAwardInSentralCommand(
    List<StudentId> StudentIds,
    IssueAwardType AwardType)
    : ICommand;