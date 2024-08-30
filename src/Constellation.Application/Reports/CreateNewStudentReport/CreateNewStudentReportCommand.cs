﻿namespace Constellation.Application.Reports.CreateNewStudentReport;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record CreateNewStudentReportCommand(
    StudentId StudentId,
    string PublishId,
    string Year,
    string ReportingPeriod,
    byte[] FileData,
    string FileName)
    : ICommand;