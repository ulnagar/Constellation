﻿namespace Constellation.Application.ThirdPartyConsent.CreateRequirement;

using Abstractions.Messaging;
using Core.Models.Subjects.Identifiers;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record CreateCourseRequirementCommand(
    ApplicationId ApplicationId,
    CourseId CourseId)
    : ICommand;