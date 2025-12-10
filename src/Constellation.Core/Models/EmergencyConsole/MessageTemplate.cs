namespace Constellation.Core.Models.EmergencyConsole;

using Enums;
using Errors;
using Identifiers;
using Shared;

public sealed class MessageTemplate
{
    private MessageTemplate() { }

    private MessageTemplate(
        MessageType type,
        string name,
        string template)
    {
        Id = new();
        TemplateType = type;
        Name = name;
        Template = template;
    }

    public TemplateId Id { get; private set; }
    public MessageType TemplateType { get; private set; }
    public string Name { get; private set; }
    public string Template { get; private set; }

    public static Result<MessageTemplate> Create(
        MessageType type,
        string name,
        string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            return Result.Failure<MessageTemplate>(MessageTemplateErrors.TemplateEmpty);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<MessageTemplate>(MessageTemplateErrors.NameEmpty);

        return new MessageTemplate(type, name, template);
    }

    public Result Update(
        string name,
        string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            return Result.Failure(MessageTemplateErrors.TemplateEmpty);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<MessageTemplate>(MessageTemplateErrors.NameEmpty);

        Name = name;
        Template = template;

        return Result.Success();
    }
}
