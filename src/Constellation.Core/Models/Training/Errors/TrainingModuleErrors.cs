namespace Constellation.Core.Models.Training.Errors;

using Identifiers;
using Shared;
using StaffMembers.Identifiers;
using System;

public static class TrainingModuleErrors
{
    public static readonly Error NoneFound = new(
        "Training.Module.NoneFound",
        "Could not find any Modules in the database");

    public static readonly Func<TrainingModuleId, Error> NotFound = id => new Error(
        "Training.Module.NotFound",
        $"A training module with the Id {id.Value} could not be found");

    public static readonly Func<StaffId, Error> AssigneeAlreadyExists = id => new(
        "Training.Module.AssigneeAlreadyExists",
        $"An Assignee with the id {id} already exists in the Module");
        
    public static readonly Func<StaffId, Error> AssigneeNotFound = id => new(
        "Training.Module.AssigneeNotFound",
        $"An Assignee with the id {id} could not be found in the Module");
}