namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactFromActiveDirectory;

using Abstractions.Messaging;

public sealed record CreateContactFromActiveDirectoryCommand(
    string EmailAddress)
    : ICommand;