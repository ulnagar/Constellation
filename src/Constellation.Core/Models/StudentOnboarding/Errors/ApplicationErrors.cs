namespace Constellation.Core.Models.StudentOnboarding.Errors;

using Enums;
using Policy;
using Shared;
using System;
using ApplicationId = Constellation.Core.Models.StudentOnboarding.Identifiers.ApplicationId;

public static class ApplicationErrors
{
    public static readonly Func<ApplicationId, Error> NotFoundById = id => new(
        "Onboarding.Application.NotFoundById",
        $"Could not find an Application with the Id '{id}'");

    public static readonly Func<ApplicationPhase, ApplicationStatus, Error> InvalidState = (phase, status) => new(
        "Onboarding.Application.InvalidState",
        $"An application state of Phase {phase} and Status {status} is not valid");

    public static readonly Func<ApplicationState, Error> TransitionBlocked = state => new(
        "Onboarding.Application.TransitionBlocked",
        $"No valid transition state available for Application in Phase {state.Phase} and Status {state.Status}");

    public static readonly Func<ApplicationState, ApplicationState, Error> TransitionInvalid = (from, to) => new(
        "Onboarding.Application.TransitionInvalid",
        $"Cannot transition from Phase {from.Phase} and Status {from.Status} to Phase {to.Phase} and Status {to.Status}");
}
