namespace Constellation.Application.Domains.Contacts.Queries.ExportContactList;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using DTOs;
using Models;
using System.Collections.Generic;

public sealed record ExportContactListCommand(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<ContactCategory> ContactCateogries,
    bool IncludeRestrictedRoles)
    : ICommand<FileDto>;
