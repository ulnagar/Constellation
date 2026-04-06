namespace Constellation.Application.Domains.Contacts.Queries.ExportContactList;

using Abstractions.Messaging;
using DTOs;
using Models;

public sealed record ExportContactListCommand(
    ContactFilter Filter,
    bool IncludeRestrictedRoles)
    : ICommand<FileDto>;
