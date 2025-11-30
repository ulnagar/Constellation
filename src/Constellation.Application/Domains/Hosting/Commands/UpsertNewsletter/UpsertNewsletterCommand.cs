namespace Constellation.Application.Domains.Hosting.Commands.UpsertNewsletter;

using Constellation.Application.Abstractions.Messaging;

public sealed record UpsertNewsletterCommand(
    int Issue,
    string Name,
    string EmbedCode)
    : ICommand;
