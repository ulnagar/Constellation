namespace Constellation.Application.SchoolContacts.CreateContactFromActiveDirectory;

using Abstractions.Messaging;

public sealed record CreateContactFromActiveDirectoryCommand(
    string EmailAddress)
    : ICommand;