﻿namespace Constellation.Application.Students.UpdateStudent;

using Abstractions.Messaging;
using Core.Enums;

public sealed record UpdateStudentCommand(
    string StudentId,
    string FirstName,
    string LastName,
    string PortalUsername,
    Grade CurrentGrade,
    string Gender,
    string SchoolCode)
    : ICommand;