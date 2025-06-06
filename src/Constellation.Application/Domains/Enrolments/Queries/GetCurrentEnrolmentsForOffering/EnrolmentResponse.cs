﻿namespace Constellation.Application.Domains.Enrolments.Queries.GetCurrentEnrolmentsForOffering;

using Core.Models.Enrolments.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record EnrolmentResponse(
    EnrolmentId EnrolmentId,
    StudentId StudentId);
