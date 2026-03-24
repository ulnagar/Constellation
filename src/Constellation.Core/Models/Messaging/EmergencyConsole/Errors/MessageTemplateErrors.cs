namespace Constellation.Core.Models.Messaging.EmergencyConsole.Errors;

using Identifiers;
using Shared;

public static class MessageTemplateErrors
{
    public static readonly Error TemplateEmpty = new(
        "EmergencyConsole.Template.Empty",
        "A Message Template must include content");

    public static readonly Error NameEmpty = new(
        "EmergencyConsole.Template.NameEmpty",
        "A Message Template must include a name");

    public static readonly Func<TemplateId, Error> NotFound = id => new(
        "EmergencyConsole.Template.NotFound",
        $"Could not find a Message Template with the Id '{id}'");

    public static readonly Func<string, Error> NameInUse = name => new(
        "EmergencyConsole.Template.NameInUse",
        $"A Template already exists with the name '{name}'");
}